using CsvHelper;
using System.Globalization;
using NodaTime;
using Microsoft.Extensions.DependencyInjection;
using OrderProcessor.Console;
using OrderProcessing.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OrderProcessing.Application.Helper;
using Serilog;


string currentDirectory = Directory.GetCurrentDirectory();
string rootDirectory = Path.GetPathRoot(currentDirectory) ?? "C:";
FileAndDirectoryHelper.CreateIODirectories(rootDirectory)

string logPath = Path.Combine(rootDirectory, "ProcessedFiles\\Log\\log-.txt");
Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information() 
            .WriteTo.File(
                path: logPath, 
                rollingInterval: RollingInterval.Day, // Daily rollover
                fileSizeLimitBytes: 1048576, // 1 MB file size limit
                retainedFileCountLimit: 7, // Keep up to 7 log files
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .CreateLogger(); 

try
{    
    Console.WriteLine("--------------------------------------------------------------------------------------------------");

    Console.WriteLine("Make sure you have you files dropped in Files/Input folder of the root directory of the Project");
    Console.WriteLine("");
    Console.WriteLine("Processing started.");

    Log.Information("Processing Input csv has been started.");
    Log.Information("--------------------------------------------------------------------------------------------------");


    using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, configuration) =>
            {
                configuration.SetBasePath(currentDirectory);
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());
                services.AddSingleton(hostContext.Configuration);
                services.ServicesDirectory();
            })
            .UseSerilog()
            .Build();

    var orderProcessor = host.Services.GetRequiredService<IOrderProcessorUseCase>();
    var status = orderProcessor.Execute();

    Console.WriteLine("Processing completed");
    Console.WriteLine("");
    Console.WriteLine("--------------------------------------------------------------------------------------------------");

    Log.Information($"Processing completed");
    Log.Information("--------------------------------------------------------------------------------------------------");
}
catch (Exception ex)
{
    Log.Error($"An error occurred: {ex.Message}");
    Console.WriteLine($"An error occurred: {ex.Message}");
    if (ex.StackTrace != null)
        Log.Error(ex.StackTrace);
}