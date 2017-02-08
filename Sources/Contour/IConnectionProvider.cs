namespace Contour
{
    internal interface IConnectionProvider<out TConnection> where TConnection: IConnection
    {
        TConnection Create();
    }
}