using OrderProcessor.Core.Entities;

namespace OrderProcessor.Infrastructure.Interfaces
{
    /// <summary>
    /// Interface for processing different types of orders
    /// Can be extended for different types of orders
    /// </summary>
    public interface IOrderProcessorRepository
    {
        IEnumerable<Output> ProcessOrders(List<Input> inputTransactions, Configuration config);
    }
}
