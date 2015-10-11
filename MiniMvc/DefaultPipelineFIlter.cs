using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMvc
{
    public class DefaultPipelineFilter : IPipelineFilter
    {
        public void BeginRequest(string controllerName, string actionName, Dictionary<string, object> parameters)
        {
        }

        public void BeforeAction(ControllerBase controller, string controllerName, string actionName, Dictionary<string, object> parameters)
        {
        }

        public void ActionError(ControllerBase controller, string controllerName, string actionName, Dictionary<string, object> parameters, Exception error)
        {
        }

        public void AfterAction(ControllerBase controller, string controllerName, string actionName, Dictionary<string, object> parameters)
        {
        }

        public void BeforeView(ControllerBase controller, string controllerName, string actionName, Dictionary<string, object> parameters)
        {
        }

        public void ViewError(ControllerBase controller, string controllerName, string actionName, Dictionary<string, object> parameters, Exception error)
        {
        }

        public void AfterView(ControllerBase controller, string controllerName, string actionName, Dictionary<string, object> parameters)
        {
        }

        public void RequestComplete(ControllerBase controller, string controllerName, string actionName, Dictionary<string, object> parameters)
        {
        }
    }
}
