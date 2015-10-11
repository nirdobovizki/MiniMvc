using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMvc
{
    public class RenderContext
    {
        public Stream BodyContent;
        public Dictionary<string, Action> Sections;
    }
}
