using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMvc
{
    public class DefaultActionFinder : IActionFinder
    {
        private class Runner : IActionRunner
        {
            private System.Reflection.MethodInfo _method;

            public Runner(System.Reflection.MethodInfo method)
            {
                _method = method;
            }

            public void Run(ControllerBase controller, Dictionary<string, object> parameters)
            {
                var paramInfo = _method.GetParameters();
                var paramValues = new object[paramInfo.Length];
                for (var i = 0; i < paramInfo.Length; ++i)
                {
                    object currentValue = null;
                    if(!parameters.TryGetValue(paramInfo[i].Name, out currentValue))
                    {
                        if (paramInfo[i].ParameterType.IsClass)
                        {
                            currentValue = null;
                        }
                        else
                        {
                            currentValue = Activator.CreateInstance(paramInfo[i].ParameterType);
                        }
                    }
                    paramValues[i] = Convert.ChangeType(currentValue, paramInfo[i].ParameterType);
                }
                _method.Invoke(controller, paramValues);
            }
        }

        public IActionRunner FindAction(ControllerBase controller, string controllerName, string actionName)
        {
            return new Runner(controller.GetType().GetMethod(actionName));
        }
    }
}
