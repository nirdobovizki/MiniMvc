using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace MiniMvc.Tests
{
    [TestClass]
    public class SharedViews
    {
        private class SimpleController : MiniMvc.ControllerBase
        {
            public void Index()
            {
                ViewBag.Name = "Index";
            }
        }

 
        [TestMethod]
        public void ViewStart()
        {
            var engine = new MiniMvc.Engine();
            var controllers = new DelegateControllerFactory();
            controllers.AddController("ctrl", () => new SimpleController());
            engine.ControllerFactory = controllers;
            var views = new StringViewLoader();
            views.AddView("ctrl/Index", "Hello @ViewBag.Name");
            views.AddView("_ViewStart", "Starting @ViewBag.Name");
            engine.ViewLoader = views;
            Assert.AreEqual("Starting IndexHello Index", RunEngine(engine, "ctrl", "Index", null));
        }
        [TestMethod]
        public void Layout()
        {
            var engine = new MiniMvc.Engine();
            var controllers = new DelegateControllerFactory();
            controllers.AddController("ctrl", () => new SimpleController());
            engine.ControllerFactory = controllers;
            var views = new StringViewLoader();
            views.AddView("ctrl/Index", "@{ Layout = \"~/Views/Shared/_Layout\"; } Hello @ViewBag.Name");
            views.AddView("~/Views/Shared/_Layout", "[[ @RenderBody() ]]");
            engine.ViewLoader = views;
            Assert.AreEqual("[[  Hello Index ]]", RunEngine(engine, "ctrl", "Index", null));
        }
        [TestMethod]
        public void Partial()
        {
            var engine = new MiniMvc.Engine();
            var controllers = new DelegateControllerFactory();
            controllers.AddController("ctrl", () => new SimpleController());
            engine.ControllerFactory = controllers;
            var views = new StringViewLoader();
            views.AddView("ctrl/Index", "Hello @RenderPartial(\"~/Shared/Partial\")");
            views.AddView("~/Shared/Partial", "Partial @ViewBag.Name");
            engine.ViewLoader = views;
            Assert.AreEqual("Hello Partial Index", RunEngine(engine, "ctrl", "Index", null));
        }
        [TestMethod]
        public void LayoutFromViewStart()
        {
            var engine = new MiniMvc.Engine();
            var controllers = new DelegateControllerFactory();
            controllers.AddController("ctrl", () => new SimpleController());
            engine.ControllerFactory = controllers;
            var views = new StringViewLoader();
            views.AddView("ctrl/Index", "Hello @ViewBag.Name");
            views.AddView("_ViewStart", "@{ Layout = \"~/Views/Shared/_Layout\"; }");
            views.AddView("~/Views/Shared/_Layout", "[[ @RenderBody() ]]");
            engine.ViewLoader = views;
            Assert.AreEqual("[[ Hello Index ]]", RunEngine(engine, "ctrl", "Index", null));
        }
        [TestMethod]
        public void Sections()
        {
            var engine = new MiniMvc.Engine();
            var controllers = new DelegateControllerFactory();
            controllers.AddController("ctrl", () => new SimpleController());
            engine.ControllerFactory = controllers;
            var views = new StringViewLoader();
            views.AddView("ctrl/Index", "@{ Layout = \"~/Views/Shared/_Layout\"; } @section Second { A } Hello @ViewBag.Name");
            views.AddView("~/Views/Shared/_Layout", "[[ @RenderBody() @RenderSection(\"Second\") ]]");
            engine.ViewLoader = views;
            Assert.AreEqual("[[   Hello Index  A  ]]", RunEngine(engine, "ctrl", "Index", null));
        }

        private string RunEngine(MiniMvc.Engine engine, string controllerName, string actionName, Dictionary<string, object> parameters)
        {
            var output = new System.IO.StringWriter();
            engine.ProcessRequest(controllerName, actionName, parameters ?? new Dictionary<string, object>(), output);
            return output.ToString();
        }
    }
}
