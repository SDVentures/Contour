namespace Contour.Configuration
{
    using System.Collections.Generic;

    /// <summary>
    ///   Трекер компонентов клиента шины. Позволяет работать с компонентами пакетно.
    /// </summary>
    public interface IBusComponentTracker
    {
        #region Public Methods and Operators

        /// <summary>
        /// Позволяет получить доступ к списку зарегистрированных компонентов.
        /// </summary>
        /// <typeparam name="T">
        /// Конкретный тип компонентов, которые необходимо получить.
        /// </typeparam>
        /// <returns>
        /// The <see cref="IEnumerable{T}"/>.
        /// </returns>
        IEnumerable<T> AllOf<T>() where T : class, IBusComponent;

        /// <summary>
        /// Зарегистрировать компонент.
        /// </summary>
        /// <param name="component">
        /// Компонент.
        /// </param>
        void Register(IBusComponent component);

        /// <summary>
        ///   Перезапуск всех зарегистрированных компонентов.
        /// </summary>
        void RestartAll();

        /// <summary>
        ///   Запуск всех зарегистрированных компонентов.
        /// </summary>
        void StartAll();

        /// <summary>
        ///   Остановка всех зарегистрированных компонентов.
        /// </summary>
        void StopAll();

        #endregion

        /// <summary>
        /// Проверка работоспособности зарегистрированных компонентов.
        /// </summary>
        /// <returns>Результат проверки.
        /// </returns>
        bool CheckHealth();
    }
}
