using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdvancedXML
{
    [TestClass]
    public class XsdSchemaTests
    {
        private const string SchemaFileName = @"BooksSchema.xsd";
        private const string ValidXmlFileName = @"Books.xml";
        private const string InvalidXmlFileName = @"BooksInvalid.xml";

        [TestMethod]
        public void XsdShcemaValidTask1()
        {
            var xmlProcess = new XsdSchema();
            var fullXmlPath = Path.Combine(@"..\..", ValidXmlFileName);
            var fullXsdPath = Path.Combine(@"..\..", SchemaFileName);
            var result = xmlProcess.XmlValidation(fullXmlPath, fullXsdPath);

            var resultMessage = GetTask1ResultMessage(result, ValidXmlFileName);
            Console.WriteLine(resultMessage);
        }

        [TestMethod]
        public void XsdShcemaInvalidTask1()
        {
            var xmlProcess = new XsdSchema();
            var fullXmlPath = Path.Combine(@"..\..", InvalidXmlFileName);
            var fullXsdPath = Path.Combine(@"..\..", SchemaFileName);
            var result = xmlProcess.XmlValidation(fullXmlPath, fullXsdPath);

            var resultMessage = GetTask1ResultMessage(result, InvalidXmlFileName);
            Console.WriteLine(resultMessage);
        }


        private string GetTask1ResultMessage(List<ValidationResult> errors, string fileName)
        {
            var message = new StringBuilder();
            message.Append($"{fileName} - ");
            if (!errors.Any())
            {
                message.AppendLine("Is Valid");
            }
            else
            {
                message.AppendLine("Is Invalid");
                foreach (var error in errors)
                {
                    message.AppendLine($"(line {error.ErrorLine}:{error.ErrorPosition}) {error.Message}");
                }
            }

            return message.ToString();
        }
    }
}
