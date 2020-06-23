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

        protected void WriteAttribute(
            string name,
            Tuple<string, int> start,
            Tuple<string, int> end,
            params object[] values)
        {
            _output.Write(start.Item1);
            foreach (var currentValue in values)
            {
                switch (currentValue)
                {
                    case Tuple<Tuple<string, int>, Tuple<string, int>, bool> t:
                        WriteAttributeInternal(t);
                        break;
                    case Tuple<Tuple<string, int>, Tuple<object, int>, bool> t:
                        WriteAttributeInternal(t);
                        break;
                    default:
                        throw new Exception("Didn't expect this type here");
                }
            }
            _output.Write(end.Item1);
        }

        private void WriteAttributeInternal<T>(Tuple<Tuple<string, int>, Tuple<T, int>, bool> value)
        {
            _output.Write(value.Item1.Item1);
            _output.Write(value.Item2.Item1);
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
