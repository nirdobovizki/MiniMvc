using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMvc
{
    public class DefaultViewLoader : IViewLoader
    {
        public bool DoesNeedToReloadView(string controllerName, string actionName, DateTime previousLoadTime)
        {
            var filename = GetFileName(controllerName, actionName);
            return new FileInfo(filename).LastWriteTimeUtc > previousLoadTime;
        }

        public System.IO.TextReader LoadViewText(string controllerName, string actionName)
        {
            var filename = GetFileName(controllerName, actionName);
            return new StreamReader(filename);

        }

        private static string GetFileName(string controllerName, string actionName)
        {
            string result = Path.Combine(
                Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location),
                "Views",
                controllerName,
                actionName + ".cshtml");
            if (!Path.GetFullPath(result).StartsWith(Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location),
                "Views"))))
            {
                throw new Exception("View outside view folder");
            }
            return result;
        }

        private static string GetSharedFileName(string sharedViewName)
        {
            string result =  Path.Combine(
                Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location),
                "Views",
                "Shared",
                sharedViewName + ".cshtml");
            if( !Path.GetFullPath(result).StartsWith(Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location),
                "Views"))))
            {
                throw new Exception("View outside view folder");
            }
            return result;
        }


        public bool DoesNeedToReloadSharedView(SharedViewRole role, string sharedViewName, DateTime previousLoadTime)
        {
            var filename = GetSharedFileName(sharedViewName);
            return new FileInfo(filename).LastWriteTimeUtc > previousLoadTime;
        }

        public TextReader LoadSharedViewText(SharedViewRole role, string sharedViewName)
        {
            var filename = GetSharedFileName(sharedViewName);
            return new StreamReader(filename);
        }

        public bool HasViewStart()
        {
            return File.Exists(GetSharedFileName("_ViewStart"));
        }
    }
}
