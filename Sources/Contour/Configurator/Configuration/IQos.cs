namespace Contour.Configurator.Configuration
{
    public interface IQos
    {
        ushort? PrefetchCount { get; }

        uint? PrefetchSize { get; }
    }
}