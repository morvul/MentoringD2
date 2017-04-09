using System.Xml;

namespace AdvancedXML
{
    public class XmlProcess
    {
        public ValidationResult XmlValidation(string xmlFileName, string xsdSchemaFileName)
        {
            var result = new ValidationResult();
            var settings = new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema
            };
            settings.Schemas.Add(null, xsdSchemaFileName);
            settings.ValidationEventHandler += (sender, e) =>
            {
                result.ErrorLine = e.Exception.LinePosition;
                result.Message = e.Message;
            };
            var reader = XmlReader.Create(xmlFileName, settings);

            while (reader.Read()){}
            return result;
        }
    }
}
