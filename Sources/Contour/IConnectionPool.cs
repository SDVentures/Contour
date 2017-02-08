namespace Contour
{
    internal interface IConnectionPool<out TConnection> where TConnection: IConnection
    {
        int MaxSize { get; }

        int Count { get; }

        TConnection Get();
    }
}