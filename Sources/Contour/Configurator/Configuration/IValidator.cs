namespace Contour.Configurator.Configuration
{
    public interface IValidator
    {
        bool Group { get; }

        string Name { get; }
    }
}