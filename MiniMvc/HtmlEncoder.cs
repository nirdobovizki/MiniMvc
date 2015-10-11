using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMvc
{
    public class HtmlEncoder : ITextEncoder
    {
        public string Encode(object source)
        {
            if (source == null) return string.Empty;
            return System.Web.HttpUtility.HtmlEncode(source.ToString());
        }
    }
}
