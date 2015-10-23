namespace Contour.Configurator
{
    using System;

    /// <summary>
    ///   Интерфейс для определения зависимостей при конфигурировании клиента шины сообщений.
    /// </summary>
    public interface IDependencyResolver
    {
        #region Public Methods and Operators

        /// <summary>
        /// Возвращает компонент по имени и типу.
        /// </summary>
        /// <param name="name">
        /// Имя компонента.
        /// </param>
        /// <param name="type">
        /// Тип компонента.
        /// </param>
        /// <returns>
        /// Найденный объект, либо null.
        /// </returns>
        object Resolve(string name, Type type);

        #endregion
    }
}
