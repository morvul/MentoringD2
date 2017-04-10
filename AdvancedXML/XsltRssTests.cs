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
        private const string HtmlFileName = @"BooksRSS.html";


        [TestMethod]
        public void XsdShcemaValidTask1()
        {
            var xmlProcess = new XsltRss();
            var fullXmlPath = Path.Combine(@"..\..", XmlFileName);
            var fullXsltPath = Path.Combine(@"..\..", XsltFileName);
            var fullHtmlPath = Path.Combine(@"..\..", HtmlFileName); 
            var htmlText = xmlProcess.XmlToRss(fullXmlPath, fullXsltPath);
            File.WriteAllText(fullHtmlPath, htmlText);
            Console.Write(htmlText);
        }
    }
}
