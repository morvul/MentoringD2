using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IQueryable.E3SClient.Entities;
using IQueryable.E3SClient;
using System.Configuration;
using System.Linq;

namespace IQueryable
{
    [TestClass]
    public class E3SProviderTests
    {
        [TestMethod]
        public void WithoutProvider()
        {
            var client = new E3SQueryClient(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);
            var res = client.SearchFts<EmployeeEntity>("workstation:(EPRUIZHW0249)", 0, 1);

            foreach (var emp in res)
            {
                Console.WriteLine("{0} {1}", emp.nativename, emp.startworkdate);
            }
        }

        [TestMethod]
        public void WithoutProviderNonGeneric()
        {
            var client = new E3SQueryClient(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);
            var res = client.SearchFts(typeof(EmployeeEntity), "workstation:(EPRUIZHW0249)", 0, 10);

            foreach (var emp in res.OfType<EmployeeEntity>())
            {
                Console.WriteLine("{0} {1}", emp.nativename, emp.startworkdate);
            }
        }


        [TestMethod]
        public void WithProvider()
        {
            var employees = new E3SEntitySet<EmployeeEntity>(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);

            foreach (var emp in employees.Where(e => e.workstation == "EPRUIZHW0249"))
            {
                Console.WriteLine("{0} {1}", emp.nativename, emp.startworkdate);
            }
        }

        [TestMethod]
        public void WithProvider_Task1()
        {
            var employees = new E3SEntitySet<EmployeeEntity>(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);

            foreach (var emp in employees.Where(e => "EPRUIZHW0249" == e.workstation))
            {
                Console.WriteLine("{0} {1}", emp.nativename, emp.startworkdate);
            }
        }

        [TestMethod]
        public void WithProvider_Task2()
        {
            var employees = new E3SEntitySet<EmployeeEntity>(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);

            Console.WriteLine("StartsWith (EPRUIZHW024):");
            foreach (var emp in employees.Where(e => e.workstation.StartsWith("EPRUIZHW024")))
            {
                Console.WriteLine("{0} {1}", emp.nativename, emp.startworkdate);
            }

            Console.WriteLine("\nEndsWith(PRUIZHW0249):");
            foreach (var emp in employees.Where(e => e.workstation.EndsWith("PRUIZHW0249")))
            {
                Console.WriteLine("{0} {1}", emp.nativename, emp.startworkdate);
            }

            Console.WriteLine("\nContains(PRUIZHW024):");
            foreach (var emp in employees.Where(e => e.workstation.Contains("PRUIZHW024")))
            {
                Console.WriteLine("{0} {1}", emp.nativename, emp.startworkdate);
            }
        }

        [TestMethod]
        public void WithProvider_Task3()
        {
            var employees = new E3SEntitySet<EmployeeEntity>(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);

            foreach (var emp in employees.Where(e => 
                e.nativename.StartsWith("Михаил") &&
                e.nativename.EndsWith("Романов") &&
                e.room == "21"))
            {
                Console.WriteLine("{0} {1}", emp.nativename, emp.startworkdate);
            }
        }
    }
}
