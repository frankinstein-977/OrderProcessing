using System.ComponentModel.DataAnnotations;

namespace OrderProcessor.Core.Entities
{
    public class Input
    {
        [Required]
        public string OrderId { get; set; }
        public string Type { get; set; }
        public int InstrumentId { get; set; }
        public string DateTime { get; set; }
        public decimal Price { get; set; } = decimal.Zero;
    }
}
