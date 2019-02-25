namespace Contour.Configurator.Configuration
{
    public interface IDigest
    {
        string Distinct { get; }

        string FlushKey { get; }

        string GroupBy { get; }

        string OutputKey { get; }

        string StoreKey { get; }

        string Type { get; }
    }
}