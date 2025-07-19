
namespace OrderProcessor.Core.Entities
{
    public class Configuration
    {
        public string Timezone { get; set; }
        public List<InstrumentsConfig> Instruments { get; set; }
    }

    public class InstrumentsConfig
    {
        public int InstrumentId { get; set; }
        public string InstrumentName { get; set; }
        public string Country { get; set; }
    }
}
