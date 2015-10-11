using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMvc
{
    public interface IPipelineFilter
    {
        void BeginRequest(string controllerName, string actionName, Dictionary<string, object> parameters);
        void BeforeAction(ControllerBase controller, string controllerName, string actionName, Dictionary<string, object> parameters);
        void ActionError(ControllerBase controller, string controllerName, string actionName, Dictionary<string, object> parameters, Exception error);
        void AfterAction(ControllerBase controller, string controllerName, string actionName, Dictionary<string, object> parameters);
        void BeforeView(ControllerBase controller, string controllerName, string actionName, Dictionary<string, object> parameters);
        void ViewError(ControllerBase controller, string controllerName, string actionName, Dictionary<string, object> parameters, Exception error);
        void AfterView(ControllerBase controller, string controllerName, string actionName, Dictionary<string, object> parameters);
        void RequestComplete(ControllerBase controller, string controllerName, string actionName, Dictionary<string, object> parameters);
    }
}
