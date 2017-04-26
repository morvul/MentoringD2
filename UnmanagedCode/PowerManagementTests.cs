using System;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnmanagedCode
{
    [TestClass]
    public class PowerManagementTests
    {
        [TestMethod]
        public void LastWakeTime_Task1()
        {
            Console.WriteLine($"Last wake time: {PowerManagement.GetLastWakeTime()}");
        }

        [TestMethod]
        public void LastSleepTime_Task1()
        {
            Console.WriteLine($"Last sleep time: {PowerManagement.GetLastSleepTime()}");
        }

        [TestMethod]
        public void GetSystemBatteryState_Task1()
        {
            var battaryState = PowerManagement.GetSystemBatteryState();
            var fields = typeof(PowerManagement.SystemBatteryState).GetFields(BindingFlags.Public | BindingFlags.Instance);
            Console.WriteLine("System Battery State:\n");
            foreach (var field in fields)
            {
                var value = field.GetValue(battaryState);
                if (field.FieldType.IsArray)
                {
                    var arrayValue = new StringBuilder();
                    foreach (var info in (Array)value)
                    {
                        if (arrayValue.Length > 0)
                        {
                            arrayValue.Append(", ");
                        }

                        arrayValue.Append(info);
                    }

                    Console.WriteLine($"{field.Name}: {arrayValue}");
                }
                else
                {
                    Console.WriteLine($"{field.Name}: {value}");
                }
            }
        }

        [TestMethod]
        public void RemoveHiberFile_Task1()
        {
            Console.WriteLine($"Hibernation file was{(PowerManagement.RemoveHiberFile() ? " succesfully" : "n't")} removed");
        }

        [TestMethod]
        public void ReserveHiberFile_Task1()
        {
            Console.WriteLine($"Hibernation file was{(PowerManagement.ReserveHiberFile() ? " succesfully" : "n't")} reserved");
        }

        [TestMethod]
        public void Sleep_Task1()
        {
            Console.WriteLine($"{(PowerManagement.Sleep() ? "Bye bye..." : "Can't sleep")}");
        }

        [TestMethod]
        public void Hibernate_Task1()
        {
            Console.WriteLine($"{(PowerManagement.Hibernate() ? "Good night..." : "Can't sleep")}");
        }
    }
}
