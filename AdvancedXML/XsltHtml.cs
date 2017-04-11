using System;
using System.IO;
using System.Xml;
using System.Xml.Xsl;

namespace AdvancedXML
{
    public class XsltHtml
    {
        public string XmlToHtml(string xmlFileName, string xsltFileName)
        {
            string result = "";
            try
            {
                XslCompiledTransform xslt = new XslCompiledTransform();
                xslt.Load(xsltFileName, new XsltSettings(false, true), new XmlUrlResolver());
                using (StringWriter sw = new StringWriter())
                using (XmlWriter writerHtml = XmlWriter.Create(sw, xslt.OutputSettings))
                {
                    xslt.Transform(xmlFileName, writerHtml);
                    result = sw.ToString();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }
    }
}
