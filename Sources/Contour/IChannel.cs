namespace Contour
{
    using System;
    using System.IO;

    /// <summary>
    ///   Канал (сессия) подключения к шине.
    /// </summary>
    public interface IChannel : IDisposable
    {
        #region Public Events

        /// <summary>
        ///   Событие для получения уведомлений о критическом сбое в работе канала.
        /// </summary>
        event Action<IChannel, ErrorEventArgs> Failed;

        #endregion

        #region Public Properties

        /// <summary>
        ///   Экземпляр шины, в рамках которого был создан канал.
        /// </summary>
        IBusContext Bus { get; }

        #endregion
    }
}
