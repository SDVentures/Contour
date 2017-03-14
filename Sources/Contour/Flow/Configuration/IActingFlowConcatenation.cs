using System;
using Contour.Caching;
using Contour.Flow.Execution;

namespace Contour.Flow.Configuration
{
    /// <summary>
    /// A message flow which enables user actions
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TSource">The source flow message type</typeparam>
    public interface IActingFlowConcatenation<TSource, TInput>: IOutgoingFlow<TSource, TInput>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="act"></param>
        /// <param name="capacity"></param>
        /// <param name="scale"></param>
        /// <param name="policy"></param>
        /// <typeparam name="TOutput"></typeparam>
        /// <returns></returns>
        IActingFlow<TSource, TOutput> Act<TOutput>(Func<FlowContext<TInput>, TOutput> act, int capacity = 1, int scale = 1, ICachePolicy policy = null) where TOutput: class;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="act"></param>
        /// <param name="capacity"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        ITerminatingFlow Act(Action<FlowContext<TInput>> act, int capacity = 1, int scale = 1);
    }
}