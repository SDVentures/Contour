using System;

namespace Contour.Flow.Configuration
{
    /// <summary>
    /// Provides a set of flow entry points
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public interface IFlowAccessor<in TInput, out TOutput>
    {
        /// <summary>
        /// Returns a new message flow entry point
        /// </summary>
        IFlowEntry<TInput> Entry();

        /// <summary>
        /// Registers a response callback and returns a new message flow entry point
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        IFlowEntry<TInput> Entry(Action<TOutput> callback);
    }

    /// <summary>
    /// Provides a set of entry points
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public interface IFlowAccessor<in TInput>
    {
        /// <summary>
        /// Returns a new message flow entry point
        /// </summary>
        IFlowEntry<TInput> Entry();
    }
}