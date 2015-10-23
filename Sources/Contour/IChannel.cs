namespace Contour
{
    using System;
    using System.IO;

    /// <summary>
    ///    анал (сесси€) подключени€ к шине.
    /// </summary>
    public interface IChannel : IDisposable
    {
        #region Public Events

        /// <summary>
        ///   —обытие дл€ получени€ уведомлений о критическом сбое в работе канала.
        /// </summary>
        event Action<IChannel, ErrorEventArgs> Failed;

        #endregion

        #region Public Properties

        /// <summary>
        ///   Ёкземпл€р шины, в рамках которого был создан канал.
        /// </summary>
        IBusContext Bus { get; }

        #endregion
    }
}
