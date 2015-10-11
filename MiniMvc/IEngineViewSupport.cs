using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMvc
{
    public interface IEngineViewSupport
    {
        string EncodeText(object source);
        void RenderPartial(string sharedViewName, TextWriter output, ViewBase view);
        void RenderSection(string sectionName, TextWriter output, ViewBase view, RenderContext renderContext);
    }
}
