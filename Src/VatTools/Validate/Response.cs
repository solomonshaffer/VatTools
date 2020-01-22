namespace VatTools.Validate
{
    public class Response
    {
        public Request Request { get; set; }
        public bool IsValid { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
    }
}