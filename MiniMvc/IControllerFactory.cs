using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMvc
{
    public interface IControllerFactory
    {
        ControllerBase CreateController(string controllerName);
    }
}
