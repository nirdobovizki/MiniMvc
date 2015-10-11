using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMvc
{
    public class Literal
    {
        private string _str;
        public Literal(string str)
        {
            _str = str;
        }
        public override string ToString()
        {
            return _str;
        }
    }
}
