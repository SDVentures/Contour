namespace Contour.Caching
{
    public interface IHasher
    {
        string GetHash(IMessage m);
    }
}