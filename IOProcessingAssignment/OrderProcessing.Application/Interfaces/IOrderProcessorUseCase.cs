using OrderProcessor.Core.Entities;

namespace OrderProcessing.Application.Interfaces
{
    public interface IOrderProcessorUseCase
    {
        Task Execute();
    }
}
