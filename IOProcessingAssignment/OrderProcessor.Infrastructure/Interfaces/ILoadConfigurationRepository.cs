using OrderProcessor.Core.Entities;

namespace OrderProcessor.Infrastructure.Interfaces
{
    /// <summary>
    /// Interface for loading settings from config.json
    /// </summary>
    public interface ILoadConfigurationRepository
    {
        Configuration? LoadConfiguration(string configFilePath);
    }
}
