using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Scripting.Hosting;

namespace Sample02_DLR
{
    [TestClass]
    public class IronPythomEmbed
    {
        [TestMethod]
        public void RunFromFile()
        {
            ScriptRuntime env = ScriptRuntime.CreateFromConfiguration();

            var scope = env.Globals;
            scope.SetVariable("b", 7);

            scope = env.ExecuteFile("IronPython.py");

            var a = scope.GetVariable<int>("a");
            Console.WriteLine(a);
        }
    }
}
