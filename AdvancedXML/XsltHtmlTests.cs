using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdvancedXML
{
    [TestClass]
    public class XsltHtmlTests
    {
        private const string XmlFileName = @"Books.xml";
        private const string XsltFileName = @"BooksHtmlTemplate.xslt";
        private const string HtmlFileName = @"BooksHtml.html";


        [TestMethod]
        public void XmlToHtmlTask3()
        {
            var xmlProcess = new XsltHtml();
            var fullXmlPath = Path.Combine(@"..\..", XmlFileName);
            var fullXsltPath = Path.Combine(@"..\..", XsltFileName);
            var fullHtmlPath = Path.Combine(@"..\..", HtmlFileName); 
            var htmlText = xmlProcess.XmlToHtml(fullXmlPath, fullXsltPath);
            File.WriteAllText(fullHtmlPath, htmlText);
            Console.Write(htmlText);
        }
    }
}
