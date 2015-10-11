using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMvc
{
    public interface IActionRunner
    {
        void Run(ControllerBase controller, Dictionary<string, object> parameters);
    }
}
