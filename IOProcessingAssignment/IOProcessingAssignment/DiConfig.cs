using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using OrderProcessing.Application.Interfaces;
using OrderProcessing.Application.UseCase;
using OrderProcessor.Infrastructure.Interfaces;
using OrderProcessor.Infrastructure.Repository;

namespace OrderProcessor.Console
{
    public static class DiConfig
    {
        /// <summary>
        /// Add all dependencies here. 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection ServicesDirectory(this IServiceCollection services)
        {
            services.AddSingleton<IDateTimeZoneProvider>(DateTimeZoneProviders.Tzdb);
            services.AddSingleton<IOrderProcessorUseCase, OrderProcessorUseCase>();
            services.AddTransient<IOrderProcessorRepository, OrderProcessorRepository>();
            services.AddTransient<ILoadConfigurationRepository, LoadConfigurationRepository>();
            return services;
        }
    }
}