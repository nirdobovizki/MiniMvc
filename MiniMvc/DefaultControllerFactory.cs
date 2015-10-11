using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMvc
{
    class DefaultControllerFactory : IControllerFactory
    {
        private Dictionary<string, Type> _controllerTypes = new Dictionary<string, Type>();
        private object _controllerTypesLock = new object();

        public ControllerBase CreateController(string controllerName)
        {
            Type controllerType = null;
            lock (_controllerTypesLock)
            {
                _controllerTypes.TryGetValue(controllerName, out controllerType);
            }

            if(controllerType == null)
            {
                var controllerTypeName = controllerName + "Controller";
                foreach(var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var type in asm.GetTypes())
                    {
                        if(type.Name == controllerTypeName)
                        {
                            controllerType = type;
                            break;
                        }
                    }
                    if (controllerType != null) break;
                }
                if (controllerType == null)
                {
                    throw new Exception("Can't find type " + controllerTypeName);
                }
                lock(_controllerTypesLock)
                {
                    _controllerTypes[controllerName] = controllerType;
                }
            }

            return (ControllerBase)Activator.CreateInstance(controllerType);
        }
    }
}
