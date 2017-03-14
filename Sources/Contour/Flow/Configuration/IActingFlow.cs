using System;
using Contour.Caching;
using Contour.Flow.Execution;

namespace Contour.Flow.Configuration
{
    /// <summary>
    /// A message flow which enables user actions, caching intermediate event broadcasting and requests
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TSource">The source flow message type</typeparam>
    public interface IActingFlow<TSource, TInput> : IOutgoingFlow<TSource, TInput>
    {
        /// <summary>
        /// Performs an action defined by <paramref name="act"/>
        /// </summary>
        /// <param name="act"></param>
        /// <param name="capacity"></param>
        /// <param name="scale"></param>
        /// <param name="policy"></param>
        /// <typeparam name="TOutput"></typeparam>
        /// <returns></returns>
        IActingFlow<TSource, TOutput> Act<TOutput>(Func<FlowContext<TInput>, TOutput> act, int capacity = 1,
            int scale = 1, ICachePolicy policy = null) where  TOutput: class;

        /// <summary>
        /// Performs an action defined by <paramref name="act"/> and passes the result down the flow
        /// </summary>
        /// <param name="act"></param>
        /// <param name="capacity"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        ITerminatingFlow Act(Action<FlowContext<TInput>> act, int capacity = 1, int scale = 1);

        /// <summary>
        /// Performs an action defined by <paramref name="act"/> and broadcasts the results
        /// </summary>
        /// <param name="act"></param>
        /// <param name="label"></param>
        /// <param name="capacity"></param>
        /// <param name="scale"></param>
        /// <param name="policy"></param>
        /// <typeparam name="TOutput"></typeparam>
        /// <returns></returns>
        IActingFlowConcatenation<TSource, TOutput> Broadcast<TOutput>(Func<FlowContext<TInput>, TOutput> act,  string label = null, int capacity = 1,
            int scale = 1, ICachePolicy policy = null) where TOutput: class;
    }
}