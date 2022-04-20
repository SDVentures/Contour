namespace Contour
{
    using System.Collections.Generic;

    using Contour.Configuration;
    using Contour.Receiving;
    using Contour.Sending;

    /// <summary>
    ///   Расширенный интерфейс шины для целей отладки.
    /// </summary>
    public interface IBusAdvanced
    {
        #region Public Properties

        /// <summary>
        ///   Трекер компонентов шины, зависящих от фактического подключения к брокеру.
        /// </summary>
        IBusComponentTracker ComponentTracker { get; }

        /// <summary>
        ///   Список получателей сообщений (по одному на каждую объявленную подписку).
        /// </summary>
        IEnumerable<IReceiver> Receivers { get; }

        /// <summary>
        ///   Список отправителей сообщений (по одному на каждое объявленное сообщение).
        /// </summary>
        IEnumerable<ISender> Senders { get; }

        #endregion
    }
}
