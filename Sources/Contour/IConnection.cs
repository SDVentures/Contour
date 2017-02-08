using System;
using System.Threading;

namespace Contour
{
    public interface IConnection: IDisposable
    {
        event EventHandler Opened;

        event EventHandler Closed;

        void Open(CancellationToken token);

        void Close();

        void Abort();
    }
}