using System.Threading.Tasks;

namespace Contour.Filters
{
    public interface IMessageExchangeFilterDecorator
    {
        Task<MessageExchange> Process(IMessageExchangeFilter filter, MessageExchange exchange, MessageExchangeFilterInvoker invoker);
    }
}