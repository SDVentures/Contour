using System;

namespace Contour
{
    /// <summary>
    /// Resolves a message label
    /// </summary>
    public interface IMessageLabelResolver
    {
        /// <summary>
        /// Resolves a message label using <paramref name="payloadType"/>
        /// </summary>
        /// <param name="payloadType"></param>
        /// <returns></returns>
        MessageLabel ResolveFrom(Type payloadType);

        /// <summary>
        /// Resolves a message label using <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        MessageLabel ResolveFrom<T>() where T : class;
    }
}