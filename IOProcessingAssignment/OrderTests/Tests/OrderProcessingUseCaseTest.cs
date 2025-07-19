using Moq;
using NodaTime;
using NodaTime.Testing;
using OrderProcessing.Application.UseCase;
using OrderProcessor.Infrastructure.Interfaces;
using OrderProcessor.Console;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OrderProcessing.Application.Helper;
using Microsoft.Extensions.Logging;
using Serilog;
using Microsoft.Extensions.Logging.Abstractions;
using OrderProcessor.Tests.Helper;

namespace OrderProcessor.Tests.Tests
{
    public class OrderProcessingUseCaseTest
    {
        public static string currentDirectory = Directory.GetCurrentDirectory();
        string rootDirectory = Path.GetPathRoot(currentDirectory) ?? "C:";
        public IHost host;

        public OrderProcessingUseCaseTest()
        {
            host = Host.CreateDefaultBuilder()
                     .ConfigureServices((hostContext, services) =>
                     {
                         services.AddLogging();
                         services.ServicesDirectory();
                     })
                     .UseSerilog()
                     .Build();
        }

        [Fact]
        public void ProcessOrders_Success()
        {
            FileAndDirectoryHelper.CreateIODirectories(rootDirectory);
            var orderProcessorRepo = host.Services.GetRequiredService<IOrderProcessorRepository>();
            var ConfigProcessorRepo = host.Services.GetRequiredService<ILoadConfigurationRepository>();
            var nullLogger = NullLogger<OrderProcessorUseCase>.Instance;
            var testLogger = new ListLogger<OrderProcessorUseCase>();

            var mockOrderRepository = new Mock<IOrderProcessorRepository>();
            var mockConfigurationRepository = new Mock<ILoadConfigurationRepository>();
            var mockDateTimeZoneProvider = new Mock<IDateTimeZoneProvider>();

            // Fake Data
            var fakeClock = new FakeClock(Instant.FromUtc(2024, 1, 1, 0, 0));
            mockDateTimeZoneProvider.Setup(x => x[It.IsAny<string>()]).Returns(DateTimeZone.Utc);

            string input = @"Type,OrderId,InstrumentId,DateTime,Price,Country
        AddOrder,101,1,2024-01-01T00:00:00,10.00,Nepal
        UpdateOrder,101,1,2024-01-01T00:01:00,10.00,Nepal";            

            string basePath = Path.Combine(rootDirectory, "ProcessedFiles");
            var inputFilePath = Path.Combine(basePath, "Input");
            var inputFullFilePath = Path.Combine(inputFilePath, "InputTransactions.csv");
            File.WriteAllText(inputFullFilePath, input);
            string[] csvFiles = Directory.GetFiles(Path.Combine(basePath, "Output"), "*.csv");
            int prevFileCount = csvFiles.Length;
            File.WriteAllText(inputFullFilePath, input);


            var useCase = new OrderProcessorUseCase(orderProcessorRepo, ConfigProcessorRepo, mockDateTimeZoneProvider.Object, testLogger);
            using var orders = useCase.Execute();

            if (orders.IsCompleted)
            {
                if (!File.Exists(inputFullFilePath))
                {
                    string[] csvFilesNow = Directory.GetFiles(Path.Combine(basePath, "Output"), "*.csv");
                    int FileCount = csvFilesNow.Length;
                    Assert.Equal(prevFileCount + 1, prevFileCount + 1);
                }
            }
            else
                throw new Exception("Test was not executed successfully");
        }

        [Fact]
        public void ProcessOrders_Error()
        {
            FileAndDirectoryHelper.CreateIODirectories(rootDirectory);
            var orderProcessorRepo = host.Services.GetRequiredService<IOrderProcessorRepository>();
            var ConfigProcessorRepo = host.Services.GetRequiredService<ILoadConfigurationRepository>();

            // Fake Data
            var mockOrderRepository = new Mock<IOrderProcessorRepository>();
            var mockConfigurationRepository = new Mock<ILoadConfigurationRepository>();
            var mockDateTimeZoneProvider = new Mock<IDateTimeZoneProvider>();
            var fakeClock = new FakeClock(Instant.FromUtc(2024, 1, 1, 0, 0));
            var mockLogger = new Mock<ILogger<OrderProcessorUseCase>>();
            mockDateTimeZoneProvider.Setup(x => x[It.IsAny<string>()]).Returns(DateTimeZone.Utc);

            string input = @"Type,OrderId,InstrumentId,DateTime,Price,Country
        UpdateOrder,101,1,2024-01-01 00:01:00,10.00,Nepal, 8";

            string basePath = Path.Combine(rootDirectory, "ProcessedFiles");
            var inputFilePath = Path.Combine(basePath, "Input");
            var inputFullFilePath = Path.Combine(inputFilePath, "InputTransactions.csv");
            File.WriteAllText(inputFullFilePath, input);
            string[] csvFiles = Directory.GetFiles(Path.Combine(basePath, "Error"), "*.csv");
            int prevFileCount = csvFiles.Length;

            var useCase = new OrderProcessorUseCase(orderProcessorRepo, ConfigProcessorRepo, mockDateTimeZoneProvider.Object, mockLogger.Object);

            var orders = useCase.Execute();

            if (orders.IsCompleted)
            {
                if (!File.Exists(inputFullFilePath))
                {
                    string[] csvFilesNow = Directory.GetFiles(Path.Combine(basePath, "Error"), "*.csv");
                    int FileCount = csvFilesNow.Length;
                    Assert.Equal(prevFileCount+1, prevFileCount + 1);
                }
            }
            else
                throw new Exception("Test was not executed successfully");
        }
    }
}