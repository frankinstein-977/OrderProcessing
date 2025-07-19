using System.Globalization;
using CsvHelper;
using Microsoft.Extensions.Logging;
using NodaTime;
using OrderProcessing.Application.Helper;
using OrderProcessing.Application.Interfaces;
using OrderProcessor.Core.Entities;
using OrderProcessor.Infrastructure.Interfaces;
using static OrderProcessor.Infrastructure.Helpers.ProcessingHelper;

namespace OrderProcessing.Application.UseCase
{
    public class OrderProcessorUseCase : IOrderProcessorUseCase
    {
        private readonly IOrderProcessorRepository _orderRepository;
        private readonly ILoadConfigurationRepository _configurationRepository;
        private readonly IDateTimeZoneProvider _dateTimeZoneProvider;
        private readonly ILogger<OrderProcessorUseCase> _logger;

        public OrderProcessorUseCase(IOrderProcessorRepository orderRepository, ILoadConfigurationRepository configurationRepository, IDateTimeZoneProvider dateTimeZoneProvider, ILogger<OrderProcessorUseCase> logger)
        {
            _orderRepository = orderRepository;
            _configurationRepository = configurationRepository;
            _dateTimeZoneProvider = dateTimeZoneProvider;
            _logger = logger;
        }
        /// <summary>
        /// Executes order processes by processing input csv 
        /// </summary>
        /// <returns></returns>
        public Task Execute()
        {
            _logger.LogInformation("Starting order processing.");

            #region Folder Configurations
            string currentDirectory = Directory.GetCurrentDirectory();
            string rootDirectory = Path.GetPathRoot(currentDirectory) ?? "C:";
            string basePath = Path.Combine(rootDirectory, "ProcessedFiles");

            var inputFilePath = Path.Combine(basePath, "Input");
            var inputFullFilePath = Path.Combine(inputFilePath, "InputTransactions.csv");
            var outputFilePath = Path.Combine(basePath, "Output");

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string projectRoot = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;
            string configFilePath = Path.Combine(projectRoot, "config.json");

            _logger.LogInformation("Folders and files located.");
            #endregion

            try
            {
                if (File.Exists(inputFullFilePath))
                {
                    var config = _configurationRepository.LoadConfiguration(configFilePath);
                    var processedOrders = new Dictionary<string, (int Revision, Instant FirstInstant)>();
                    var outputRecords = new List<Output>();

                    using (var reader = new StreamReader(inputFullFilePath))
                    {
                        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                        {
                            var inputRecordMap = csv.Context.RegisterClassMap<InputRecordMap>();
                            var records = csv.GetRecords<Input>().ToList();

                            _logger.LogInformation("Orders fetched from Input file.");
                            outputRecords = _orderRepository.ProcessOrders(records, config).ToList();
                        }
                    }

                    if (outputRecords.Count > 0)
                    {
                        var outputFullFilePath = Path.Combine(outputFilePath, "output_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".csv");
                        using (var writer = new StreamWriter(outputFullFilePath))
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            csv.WriteRecords(outputRecords);
                        }
                        _logger.LogInformation("Orders written in output file.");

                        if (File.Exists(outputFullFilePath))
                        {
                            FileAndDirectoryHelper.ProcessingSuccess(basePath, inputFullFilePath);
                            _logger.LogInformation($"Input file archived to {Path.Combine(basePath, "Archive")}.");
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"Error in the input data. No transactions were able to be converted to respective output.");
                        FileAndDirectoryHelper.ProcessingFailure(basePath, inputFullFilePath);
                        _logger.LogInformation($"Faulty Input file sent to {Path.Combine(basePath, "Error")}");
                    }
                }
                else
                {
                    _logger.LogError($"No input data found in {inputFilePath}.");
                }
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error occurred while processing files. Error: '{ex.Message}' in '{ex.StackTrace}'.");
                FileAndDirectoryHelper.ProcessingFailure(basePath, inputFullFilePath);
                _logger.LogInformation($"Faulty Input file sent to Error.");
                return Task.CompletedTask;
            }
        }
    }

}
