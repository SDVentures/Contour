namespace Contour.Caching
{
    public interface IKeyProvider
    {
        string Get(object input);
    }
}