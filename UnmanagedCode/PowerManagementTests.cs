using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnmanagedCode
{
    [TestClass]
    public class PowerManagementTests
    {
        [TestMethod]
        public void LastWakeTime_Task1()
        {
            Console.WriteLine(PowerManagement.GetLastWakeTime());
        }
    }
}
