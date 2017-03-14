using System;
using Contour.Flow.Execution;

namespace Contour.Flow.Configuration
{
    /// <summary>
    /// Provides a set of flow entry points
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TInput"></typeparam>
    public interface IFlowAccessor<TInput, TOutput>
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
        IFlowEntry<TInput> Entry(Action<FlowContext<TOutput>> callback);
    }

    /// <summary>
    /// Provides a set of entry points
    /// </summary>
    public interface IFlowAccessor<TInput>
    {
        /// <summary>
        /// Returns a new message flow entry point
        /// </summary>
        IFlowEntry<TInput> Entry();
    }
}