using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMvcTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var engine = new MiniMvc.Engine();
            var output = new System.IO.StringWriter();
            engine.ProcessRequest("Demo", "Index", new Dictionary<string, object> { { "One", 1 }, { "Two", 2 } }, output);
            Console.WriteLine(output.ToString());
            Console.ReadKey();
        }
    }

    public class DemoController : MiniMvc.ControllerBase
    {
        public void Index(int One, int Two)
        {
            ViewBag.One = One;
            ViewBag.Two = Two;
        }
    }
}
