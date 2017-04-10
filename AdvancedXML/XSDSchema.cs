using System.Collections.Generic;
using System.Xml;

namespace AdvancedXML
{
    public class XsdSchema
    {
        public List<ValidationResult> XmlValidation(string xmlFileName, string xsdSchemaFileName)
        {
            var errors = new List<ValidationResult>();
            var settings = new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema
            };
            settings.Schemas.Add(null, xsdSchemaFileName);
            settings.ValidationEventHandler += (sender, e) =>
            {
                var error = new ValidationResult
                {
                    ErrorLine = e.Exception.LineNumber,
                    ErrorPosition = e.Exception.LinePosition,
                    Message = e.Message
                };
                errors.Add(error);
            };
            var reader = XmlReader.Create(xmlFileName, settings);

            while (reader.Read()){}
            return errors;
        }
    }
}
