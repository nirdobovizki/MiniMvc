using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Razor;

namespace MiniMvc
{
    public class Engine : IEngineViewSupport
    {
        private class CachedView
        {
            public DateTime LoadTimestamp;
            public Type ViewType;
        }

        private RazorEngineHost _host;
        private static int _count;
        private Dictionary<string, CachedView> _loadedViews = new Dictionary<string, CachedView>();
        private object _loadedViewsLock = new object();
        private IControllerFactory _controllerFactory = new DefaultControllerFactory();
        private IViewLoader _viewLoader = new DefaultViewLoader();
        private IActionFinder _actionFinder = new DefaultActionFinder();
        private IPipelineFilter _pipelineFilter = new DefaultPipelineFilter();
        private ITextEncoder _textEncoder = new HtmlEncoder();
        private bool _streamingMode;

        public IControllerFactory ControllerFactory { get { return _controllerFactory; } set { _controllerFactory = value; } }
        public IViewLoader ViewLoader { get { return _viewLoader; } set { _viewLoader = value; } }
        public IActionFinder ActionFinder { get { return _actionFinder; } set { _actionFinder = value; } }
        public IPipelineFilter PipelineFilter { get { return _pipelineFilter; } set { _pipelineFilter = value; } }
        public ITextEncoder TextEncoder { get { return _textEncoder; } set { _textEncoder = value; } }
        public bool StreamingMode { get { return _streamingMode; } set { _streamingMode = value; } }
        public List<string> References { get; } = new List<string>();

        public Engine()
        {
            _host = new RazorEngineHost(new CSharpRazorCodeLanguage());
            _host.DefaultBaseClass = "MiniMvc.ViewBase";
            _host.NamespaceImports.Add("System");
            _host.GeneratedClassContext = new System.Web.Razor.Generator.GeneratedClassContext("Execute", "Write", "WriteLiteral", null, null, null, "DefineSection", null, null);
        }

        public void ProcessRequest(string controllerName, string actionName, Dictionary<string, object> parameters, TextWriter output)
        {
            MemoryStream bodyContent = _streamingMode ? null : new MemoryStream();
            TextWriter bodyWriter = _streamingMode ? null : new StreamWriter(bodyContent);

            _pipelineFilter.BeginRequest(controllerName, actionName, parameters);

            var controller = _controllerFactory.CreateController(controllerName);
            InitializeController(controller);
            var actionRunner = _actionFinder.FindAction(controller, controllerName, actionName);
            ExecuteActionWithFilter(controllerName, actionName, parameters, controller, actionRunner);
            string layoutName = null;
            if(ViewLoader.HasViewStart())
            {
                var viewStart = FindAndExecuteSharedView(SharedViewRole.ViewStart, "_ViewStart", _streamingMode ? output : bodyWriter, controller, null, null, null);
                layoutName = viewStart.Layout;
            }
            var view = FindAndExecuteView(controllerName, actionName, parameters, _streamingMode ? output : bodyWriter, controller, layoutName);

            if(view.Layout!=null)
            {
                if(_streamingMode)
                {
                    throw new Exception("Can't use layouts in streaming mode, set MiniMvc.Engine.StreamingMode to false");
                }
                bodyWriter.Flush();
                view.InitializeView(output, this, new RenderContext());
                FindAndExecuteSharedView(SharedViewRole.Layout, view.Layout, output, null, view, bodyContent, view.Sections);
            }
            else
            {
                if(!_streamingMode)
                {
                    bodyWriter.Flush();
                    bodyContent.Seek(0, SeekOrigin.Begin);
                    var bodyReader = new StreamReader(bodyContent);
                    CopyStream(bodyReader, output);
                }
            }

            _pipelineFilter.RequestComplete(controller, controllerName, actionName, parameters);
        }

        private void CopyStream(StreamReader input, TextWriter output)
        {
            char[] buffer = new char[1024];
            int count = input.Read(buffer, 0, buffer.Length);
            while(count>0)
            {
                output.Write(buffer, 0, count);
                count = input.Read(buffer, 0, buffer.Length);
            }
        }

        private ViewBase FindAndExecuteSharedView(SharedViewRole role, string sharedViewName, TextWriter output, ControllerBase controller, ViewBase parentView, Stream bodyContent, Dictionary<string,Action> sections)
        {
            var viewKey = "/#" + sharedViewName;
            CachedView viewData;
            viewData = GetCachedView(viewKey);
            if (viewData != null)
            {
                viewData = CheckCachedSharedViewValidity(role, sharedViewName, viewData);
            }
            if (viewData == null)
            {
                viewData = LoadSharedView(role, sharedViewName, viewKey, viewData);
                SaveViewInCache(viewKey, viewData);
            }
            var view = (ViewBase)Activator.CreateInstance(viewData.ViewType);
            InitializeView(controller, parentView, view, output, new RenderContext { BodyContent = bodyContent,Sections = sections });
            view.Execute();
            return view;
        }
        private ViewBase FindAndExecuteView(string controllerName, string actionName, Dictionary<string, object> parameters, TextWriter output, ControllerBase controller, string layoutName)
        {
            var viewKey = controllerName + "/" + actionName;
            CachedView viewData;
            viewData = GetCachedView(viewKey);
            if (viewData != null)
            {
                viewData = CheckCachedViewValidity(controllerName, actionName, viewData);
            }
            if (viewData == null)
            {
                viewData = LoadView(controllerName, actionName, viewKey, viewData);
                SaveViewInCache(viewKey, viewData);
            }
            var view = (ViewBase)Activator.CreateInstance(viewData.ViewType);
            view.Layout = layoutName;
            InitializeView(controller, null, view, output, new RenderContext());
            ExecuteViewWithFilter(controllerName, actionName, parameters, controller, view);
            return view;
        }

        private void SaveViewInCache(string viewKey, CachedView viewData)
        {
            lock (_loadedViewsLock)
            {
                _loadedViews[viewKey] = viewData;
            }
        }

        private CachedView LoadView(string controllerName, string actionName, string viewKey, CachedView viewData)
        {
            viewData = new CachedView();
            using (var viewText = _viewLoader.LoadViewText(controllerName, actionName))
            {
                viewData.ViewType = PrepareView(viewText, viewKey);
            }
            viewData.LoadTimestamp = DateTime.UtcNow;
            return viewData;
        }

        private CachedView LoadSharedView(SharedViewRole role, string sharedViewName, string viewKey, CachedView viewData)
        {
            viewData = new CachedView();
            using (var viewText = _viewLoader.LoadSharedViewText(role, sharedViewName))
            {
                viewData.ViewType = PrepareView(viewText, viewKey);
            }
            viewData.LoadTimestamp = DateTime.UtcNow;
            return viewData;
        }


        private CachedView CheckCachedViewValidity(string controllerName, string actionName, CachedView viewData)
        {
            if (_viewLoader.DoesNeedToReloadView(controllerName, actionName, viewData.LoadTimestamp))
            {
                viewData = null;
            }
            return viewData;
        }

        private CachedView CheckCachedSharedViewValidity(SharedViewRole role, string sharedViewName, CachedView viewData)
        {
            if (_viewLoader.DoesNeedToReloadSharedView(role, sharedViewName, viewData.LoadTimestamp))
            {
                viewData = null;
            }
            return viewData;
        }

        private CachedView GetCachedView(string viewKey)
        {
            CachedView viewData;
            lock (_loadedViewsLock)
            {
                _loadedViews.TryGetValue(viewKey, out viewData);
            }
            return viewData;
        }

        private void ExecuteActionWithFilter(string controllerName, string actionName, Dictionary<string, object> parameters, ControllerBase controller, IActionRunner actionRunner)
        {
            _pipelineFilter.BeforeAction(controller, controllerName, actionName, parameters);
            try
            {
                actionRunner.Run(controller, parameters);
            }
            catch (Exception ex)
            {
                _pipelineFilter.ActionError(controller, controllerName, actionName, parameters, ex);
                throw;
            }
            _pipelineFilter.AfterAction(controller, controllerName, actionName, parameters);
        }

        private void ExecuteViewWithFilter(string controllerName, string actionName, Dictionary<string, object> parameters, ControllerBase controller, ViewBase view)
        {
            _pipelineFilter.BeforeView(controller, controllerName, actionName, parameters);
            try
            {
                view.Execute();
            }
            catch (Exception ex)
            {
                _pipelineFilter.ViewError(controller, controllerName, actionName, parameters, ex);
                throw;
            }
            _pipelineFilter.AfterView(controller, controllerName, actionName, parameters);
        }

        private void InitializeView(ControllerBase controller, ViewBase parentView, ViewBase view, TextWriter output, RenderContext renderContext)
        {
            if (controller != null)
            {
                view.ViewBag = controller.ViewBag;
            }
            else
            {
                view.ViewBag = parentView.ViewBag;
            }
            view.InitializeView(output, this, renderContext);
        }

        private void InitializeController(ControllerBase controller)
        {
            controller.ViewBag = new DynamicDictionary();
        }

        public Type PrepareView(TextReader viewText, string viewFileName)
        {
            var currrentCount = Interlocked.Increment(ref _count);
            var className = "_view" + currrentCount.ToString(CultureInfo.InvariantCulture);
            RazorTemplateEngine razor = new RazorTemplateEngine(_host);
            var parseResult = razor.GenerateCode(viewText, className, "MiniMVC._generated", viewFileName);
            if (!parseResult.Success)
            {
                throw new ViewCompilerException(parseResult.ParserErrors, viewFileName);
            }
            var codeProvider = new CSharpCodeProvider();
            var compilerParams = new CompilerParameters(new string[] 
            {
                typeof(Tuple).Assembly.Location,
                typeof(Engine).Assembly.Location,
                typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).Assembly.Location,
                typeof(System.Runtime.CompilerServices.CallSite).Assembly.Location,
            }.Concat(References).ToArray());

            compilerParams.GenerateInMemory = true;

            var code = new StringWriter();
            codeProvider.GenerateCodeFromCompileUnit(parseResult.GeneratedCode, code, new CodeGeneratorOptions());
            System.Diagnostics.Debug.Write(code.ToString());
                
            var compileResult = codeProvider.CompileAssemblyFromDom(compilerParams, parseResult.GeneratedCode);
            if (compileResult.Errors.HasErrors)
            {
                throw new ViewCompilerException(compileResult.Errors, viewFileName);
            }

            return compileResult.CompiledAssembly.GetType("MiniMVC._generated." + className);
        }

        string IEngineViewSupport.EncodeText(object source)
        {
            return _textEncoder.Encode(source);
        }

        void IEngineViewSupport.RenderPartial(string sharedViewName, TextWriter output, ViewBase view)
        {
            FindAndExecuteSharedView(SharedViewRole.Partial, sharedViewName, output, null, view, null, null);
        }


        void IEngineViewSupport.RenderSection(string sectionName, TextWriter output, ViewBase view, RenderContext renderContext)
        {
            if(sectionName == null)
            {
                renderContext.BodyContent.Seek(0, SeekOrigin.Begin);
                var reader = new StreamReader(renderContext.BodyContent);
                CopyStream(reader, output);
            }
        }
    }
}
