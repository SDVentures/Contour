namespace Contour.Configurator.Configuration
{
    public interface IBusConfigurationSection
    {
        IEndpoint[] Endpoints { get; }
    }
}