using System;

namespace Contour.Configuration.Configurator
{
    /// <summary>
    ///   Объект-заглушка для получения зависимостей для построение клиента шины сообщений. Кидает исключение при любом
    ///   использовании.
    /// </summary>
    public class StubDependencyResolver : IDependencyResolver
    {
        /// <summary>
        /// The resolve.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public object Resolve(string name, Type type)
        {
            return new InvalidOperationException("No IDependencyResolver provided.");
        }
    }
}
