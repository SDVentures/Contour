using System;

namespace Contour.Configurator.Configuration
{
    public interface IEndpoint
    {
        ICaching Caching { get; }

        IQos Qos { get; }

        IDynamic Dynamic { get; }

        string ConnectionString { get; }

        bool? ReuseConnection { get; }

        uint? ParallelismLevel { get; }

        TimeSpan? FaultQueueTtl { get; }

        int? FaultQueueLimit { get; }

        string ConnectionStringProvider { get; }

        IIncoming[] Incoming { get; }

        string LifecycleHandler { get; }

        string Name { get; }

        IOutgoing[] Outgoing { get; }

        IValidator[] Validators { get; }

        string[] ExcludedHeaders { get; }
    }
}