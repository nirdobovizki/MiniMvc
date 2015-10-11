using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMvc
{
    public interface IViewLoader
    {
        bool HasViewStart();
        bool DoesNeedToReloadView(string controllerName, string actionName, DateTime previousLoadTime);
        TextReader LoadViewText(string controllerName, string actionName);
        bool DoesNeedToReloadSharedView(SharedViewRole role, string sharedViewName, DateTime previousLoadTime);
        TextReader LoadSharedViewText(SharedViewRole role, string sharedViewName);
    }
}
