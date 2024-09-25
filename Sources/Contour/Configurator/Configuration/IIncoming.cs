namespace Contour.Configurator.Configuration
{
    public interface IIncoming : IMessage
    {
        Lifestyle? Lifestyle { get; }

        string React { get; }

        bool RequiresAccept { get; }

        string Type { get; }

        string Validate { get; }

        IQos Qos { get; }

        uint? ParallelismLevel { get; }

        string ConnectionString { get; }

        bool? ReuseConnection { get; }

        int? QueueLimit { get; }

        int? QueueMaxLengthBytes { get; }

        bool Delayed { get; }

        bool Direct { get; }

        string DirectId { get; }
    }
}