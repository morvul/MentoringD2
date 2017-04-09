using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdvancedXML
{
    [TestClass]
    public class XmlProcessTests
    {
        private const string SchemaFileName = @"BooksSchema.xsd";
        private const string ValidXmlFileName = @"Books.xml";
        private const string InvalidXmlFileName = @"BooksInvalid.xml";

        [TestMethod]
        public void XsdShcemaValidTask1()
        {
            var xmlProcess = new XmlProcess();
            var fullXmlPath = Path.Combine(@"..\..", ValidXmlFileName);
            var fullXsdPath = Path.Combine(@"..\..", SchemaFileName);
            var result = xmlProcess.XmlValidation(fullXmlPath, fullXsdPath);

            var resultMessage = GetTask1ResultMessage(result, ValidXmlFileName);
            Console.WriteLine(resultMessage);
        }

        [TestMethod]
        public void XsdShcemaInvalidTask1()
        {
            var xmlProcess = new XmlProcess();
            var fullXmlPath = Path.Combine(@"..\..", InvalidXmlFileName);
            var fullXsdPath = Path.Combine(@"..\..", SchemaFileName);
            var result = xmlProcess.XmlValidation(fullXmlPath, fullXsdPath);

            var resultMessage = GetTask1ResultMessage(result, InvalidXmlFileName);
            Console.WriteLine(resultMessage);
        }


        private string GetTask1ResultMessage(ValidationResult result, string fileName)
        {
            var message = new StringBuilder();
            message.Append($"{fileName} - ");
            if (result.IsValid)
            {
                message.AppendLine("Is Valid");
            }
            else
            {
                message.AppendLine("Is Invalid");
                message.AppendLine($"(line {result.ErrorLine}) {result.Message}");
            }

            return message.ToString();
        }
    }
}
