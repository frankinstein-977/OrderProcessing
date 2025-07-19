namespace OrderProcessor.Core.Entities
{
    public class Output
    {
        public string OrderId { get; set; }
        public string Type { get; set; }
        public int Revision { get; set; }
        public string DateTimeUtc { get; set; }
        public decimal Price { get; set; }
        public string Country { get; set; }
        public string InstrumentName { get; set; }
    }
}
