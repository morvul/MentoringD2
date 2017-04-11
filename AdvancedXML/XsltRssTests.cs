using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdvancedXML
{
    [TestClass]
    public class XsltRssTests
    {
        private const string XmlFileName = @"Books.xml";
        private const string XsltFileName = @"BooksRssTemplate.xslt";

        [TestMethod]
        public void XmlToRssTask2()
        {
            var xmlProcess = new XsltRss();
            var fullXmlPath = Path.Combine(@"..\..", XmlFileName);
            var fullXsltPath = Path.Combine(@"..\..", XsltFileName);
            var htmlText = xmlProcess.XmlToRss(fullXmlPath, fullXsltPath);
            Console.Write(htmlText);
        }
    }
}
