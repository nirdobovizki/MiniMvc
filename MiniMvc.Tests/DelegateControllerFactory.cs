using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMvc.Tests
{
    class DelegateControllerFactory : MiniMvc.IControllerFactory
    {
        public delegate ControllerBase Factory();

        private Dictionary<string, Factory> _factories = new Dictionary<string, Factory>();

        public void AddController(string name, Factory fac)
        {
            _factories.Add(name,fac);
        }

        public ControllerBase CreateController(string controllerName)
        {
            return _factories[controllerName]();
        }
    }
}
