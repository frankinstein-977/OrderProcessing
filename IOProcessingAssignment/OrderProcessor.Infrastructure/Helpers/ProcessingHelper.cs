using CsvHelper.Configuration;
using OrderProcessor.Core.Entities;

namespace OrderProcessor.Infrastructure.Helpers
{
    public class ProcessingHelper
    {
        /// <summary>
        /// CSV fields mapping
        /// </summary>
        public class InputRecordMap : ClassMap<Input>
        {
            public InputRecordMap()
            {
                Map(m => m.Type).Name("Type");
                Map(m => m.OrderId).Name("OrderId");
                Map(m => m.InstrumentId).Name("InstrumentId");
                Map(m => m.DateTime).Name("DateTime");
                Map(m => m.Price).Name("Price");
            }
        }
    }
}
