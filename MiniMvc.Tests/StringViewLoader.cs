using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMvc.Tests
{
    class StringViewLoader : MiniMvc.IViewLoader
    {
        private Dictionary<string, string> _views = new Dictionary<string, string>();

        public void AddView(string key, string view)
        {
            _views.Add(key, view);
        }

        public bool DoesNeedToReloadView(string controllerName, string actionName, DateTime previousLoadTime)
        {
            return false;
        }

        public System.IO.TextReader LoadViewText(string controllerName, string actionName)
        {
            return new System.IO.StringReader(_views[controllerName + "/" + actionName]);
        }


        public bool DoesNeedToReloadSharedView(SharedViewRole role, string sharedViewName, DateTime previousLoadTime)
        {
            return false;
        }

        public System.IO.TextReader LoadSharedViewText(SharedViewRole role, string sharedViewName)
        {
            return new System.IO.StringReader(_views[sharedViewName]);
        }

        public bool HasViewStart()
        {
            return _views.ContainsKey("_ViewStart");
        }
    }
}
