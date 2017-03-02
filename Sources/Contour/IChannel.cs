namespace Contour
{
    using System;
    using System.IO;

    /// <summary>
    ///   Канал (сессия) подключения к шине.
    /// </summary>
    public interface IChannel : IDisposable
    {
        /// <summary>
        ///   Событие для получения уведомлений о критическом сбое в работе канала.
        /// </summary>
        event Action<IChannel, ErrorEventArgs> Failed;
    }
}
