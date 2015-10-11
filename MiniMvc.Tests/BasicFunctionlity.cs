using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace MiniMvc.Tests
{
    [TestClass]
    public class BasicFunctionlity
    {
        private class SimpleController : MiniMvc.ControllerBase
        {
            public void Index()
            {
                ViewBag.Name = "Index";
            }

            public void WithParameters(int p1, double p2, string p3)
            {
                ViewBag.p1 = p1;
                ViewBag.p2 = p2;
                ViewBag.p3 = p3;
            }

            public void Encode1()
            {
                ViewBag.Str = "<";
            }

            public void Encode2()
            {
                ViewBag.Str = "<";
            }


        }

        MiniMvc.Engine engine = new Engine();

        public BasicFunctionlity()
        {
            var views = new StringViewLoader();
            views.AddView("ctrl/Index", "Hello @ViewBag.Name");
            views.AddView("ctrl/WithParameters", "this is an int @ViewBag.p1");
            views.AddView("ctrl/Encode1", "<@ViewBag.Str");
            views.AddView("ctrl/Encode2", "<@Raw(ViewBag.Str)");
            engine.ViewLoader = views;
            var controllers = new DelegateControllerFactory();
            controllers.AddController("ctrl", () => new SimpleController());
            engine.ControllerFactory = controllers;

        }

        [TestMethod]
        public void NoParams()
        {
            Assert.AreEqual("Hello Index", RunEngine("ctrl", "Index", null));
        }

        [TestMethod]
        public void WithParams()
        {
            Assert.AreEqual("this is an int 5", RunEngine("ctrl", "WithParameters", new Dictionary<string, object> { { "p1", 5 }, { "p2", 3.2 }, { "p3", "str" } }));
        }

        [TestMethod]
        public void MissingParams()
        {
            Assert.AreEqual("this is an int 0", RunEngine("ctrl", "WithParameters", null));
        }


        [TestMethod]
        public void HtmlEncode()
        {
            Assert.AreEqual("<&lt;", RunEngine("ctrl", "Encode1", null));
        }

        [TestMethod]
        public void Raw()
        {
            Assert.AreEqual("<<", RunEngine("ctrl", "Encode2", null));
        }


        private string RunEngine(string controllerName, string actionName, Dictionary<string, object> parameters)
        {
            var output = new System.IO.StringWriter();
            engine.ProcessRequest(controllerName, actionName, parameters ?? new Dictionary<string, object>(), output);
            return output.ToString();
        }
    }
}
