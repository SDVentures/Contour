using System;

namespace Contour.Configurator.Configuration
{
    public interface IOutgoing : IMessage
    {
        ICallbackEndpoint CallbackEndpoint { get; }

        bool Confirm { get; }

        bool Persist { get; }

        TimeSpan? Timeout { get; }

        TimeSpan? Ttl { get; }

        string ConnectionString { get; }

        bool? ReuseConnection { get; }

        bool Delayed { get; }

        bool Direct { get; }

        TimeSpan? ConfirmTimeout { get; }
    }
}