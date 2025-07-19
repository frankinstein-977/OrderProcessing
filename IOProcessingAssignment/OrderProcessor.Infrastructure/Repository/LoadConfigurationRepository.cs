using OrderProcessor.Core.Entities;
using Newtonsoft.Json;
using OrderProcessor.Infrastructure.Interfaces;

namespace OrderProcessor.Infrastructure.Repository
{
    public class LoadConfigurationRepository:ILoadConfigurationRepository
    {
        /// <summary>
        /// De-serializes config file
        /// </summary>
        /// <param name="configFilePath"></param>
        /// <returns></returns>
        public Configuration? LoadConfiguration(string configFilePath)
        {
            try
            {
                string json = File.ReadAllText(configFilePath);
                return JsonConvert.DeserializeObject<Configuration>(json);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON parsing error: {ex.Message}");
                return null;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"File reading error: {ex.Message}");
                return null;
            }
        }
    }
}
