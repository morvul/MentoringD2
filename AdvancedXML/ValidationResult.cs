namespace AdvancedXML
{
    public class ValidationResult
    {
        public int? ErrorLine { get; set; }

        public string Message { get; set; }

        public bool IsValid => string.IsNullOrEmpty(Message) && ErrorLine == null;
    }
}
