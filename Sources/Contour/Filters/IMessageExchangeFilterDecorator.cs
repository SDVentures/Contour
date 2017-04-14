namespace Contour.Filters
{
    using System.Threading.Tasks;

    public interface IMessageExchangeFilterDecorator
    {
        Task<MessageExchange> Process(IMessageExchangeFilter filter, MessageExchange exchange, MessageExchangeFilterInvoker invoker);
    }
}