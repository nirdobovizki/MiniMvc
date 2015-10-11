using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMvc
{
    public class ViewBase
    {
        private TextWriter _output;
        private IEngineViewSupport _engine;
        private RenderContext _renderContext;
        public dynamic ViewBag;
        public string Layout;
        public Dictionary<string, Action> Sections = new Dictionary<string, Action>();

        public virtual void Execute()
        {

        }

        public void InitializeView(TextWriter output,IEngineViewSupport engine, RenderContext renderContext)
        {
            _output = output;
            _engine = engine;
            _renderContext = renderContext;
        }

        protected void Write(object value)
        {
            if (value is Literal)
            {
                _output.Write(value.ToString());
            }
            else
            {
                _output.Write(_engine.EncodeText(value));
            }
        }

        protected void WriteLiteral(object value)
        {
            _output.Write(value);
        }

        protected Literal Raw(string source)
        {
            return new Literal(source);
        }

        protected object RenderPartial(string sharedViewName)
        {
            _engine.RenderPartial(sharedViewName, _output, this);
            return null;
        }

        protected object RenderBody()
        {
            _engine.RenderSection(null, _output, this, _renderContext);
            return null;
        }

        protected void DefineSection(string sectionName, Action executeSection)
        {
            Sections.Add(sectionName, executeSection);
        }

        protected object RenderSection(string sectionName)
        {
            if(_renderContext.Sections!=null)
            {
                Action sectionAction;
                if(_renderContext.Sections.TryGetValue(sectionName,out sectionAction))
                {
                    sectionAction();
                }
            }
            return null;
        }
    }
}
