using System.Globalization;
using NodaTime;
using NodaTime.Text;
using OrderProcessor.Infrastructure.Interfaces;
using OrderProcessor.Core.Entities;

namespace OrderProcessor.Infrastructure.Repository
{
    /// <summary>
    /// Process the input and converts to desired output
    /// </summary>
    public class OrderProcessorRepository : IOrderProcessorRepository
    {
        private readonly ILoadConfigurationRepository _configuration;
        private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

        public OrderProcessorRepository(ILoadConfigurationRepository configuration, IDateTimeZoneProvider dateTimeZoneProvider)
        {
            _configuration = configuration;
            _dateTimeZoneProvider = dateTimeZoneProvider;
        }

        public IEnumerable<Output> ProcessOrders(List<Input> inputTransactions, Configuration config)
        {
            var processedOrders = new Dictionary<string, (int Revision, Instant FirstInstant)>();
            var outputRecords = new List<Output>();
            try
            {
                foreach (var record in inputTransactions)
                {
                    //OrderId, Price combination for ignoring records with same OrderId and same price, but different timestamp 
                    var orderKey = $"{record.OrderId}-{record.Price}";
                    int revision = 1;

                    var pattern = LocalDateTimePattern.CreateWithInvariantCulture(patternText: "yyyy-MM-ddTHH:mm:ss.FFFFFFF");
                    var parseResult = pattern.Parse(record.DateTime);

                    if (!parseResult.Success)
                    {
                        Console.WriteLine($"Error parsing DateTime: {record.DateTime} for OrderId: {record.OrderId}. Error: {parseResult.Exception.Message}");
                        continue; // Skip the error prone record
                    }

                    var localDateTime = parseResult.Value;

                    var sourceTimeZone = _dateTimeZoneProvider[config.Timezone];
                    var zonedInputDateTime = localDateTime.InZoneStrictly(sourceTimeZone);
                    var instant = zonedInputDateTime.ToInstant();

                    // Calculating Revision
                    var existingOrder = outputRecords.Where(x => x.OrderId == record.OrderId);
                    revision = existingOrder?.Count() > 0 ? existingOrder.Last().Revision + 1 : revision;

                    if (!processedOrders.TryGetValue(orderKey, out var processedData) || instant < processedData.FirstInstant)
                    {
                        processedOrders[orderKey] = (revision, instant);

                        string country = "Error"; string insName = "Error";
                        var instrument = config.Instruments.FirstOrDefault(i => i.InstrumentId == record.InstrumentId);
                        if (instrument != null)
                        {
                            if (string.IsNullOrEmpty(instrument.Country))
                                Console.WriteLine($"Country not found for OrderId: {record.OrderId}. InstrumentId: {instrument.InstrumentId}");
                            else
                                country = instrument.Country;

                            if (string.IsNullOrEmpty(instrument.InstrumentName))
                                Console.WriteLine($"Instrument Name not found for OrderId: {record.OrderId}. InstrumentId: {instrument.InstrumentId}");
                            else
                                insName = instrument.InstrumentName;
                        }

                        outputRecords.Add(new Output
                        {
                            OrderId = record.OrderId,
                            Type = record.Type switch
                            {
                                "AddOrder" => "ADD",
                                "UpdateOrder" => "UPDATE",
                                "DeleteOrder" => "DELETE",
                                _ => "UNSPECIFIED"
                            },
                            Revision = revision,
                            DateTimeUtc = instant.ToDateTimeUtc().ToString("o", CultureInfo.InvariantCulture),
                            Price = record.Price,
                            Country = country,
                            InstrumentName = insName
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return outputRecords;
        }
    }
}
