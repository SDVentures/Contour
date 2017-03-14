using System;

namespace Contour.Flow.Execution
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Provides a default implementation of the flow context
    /// </summary>
    /// <typeparam name="TValue">The type of the value held in flow context</typeparam>
    public class FlowContext<TValue> : FlowContext
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public TValue Value { get; set; }

        /// <summary>
        /// Extracts the head of the flow context chain
        /// </summary>
        /// <returns>
        /// The <see cref="Guid"/>.
        /// </returns>
        public override Guid Head()
        {
            return this.Tail?.Head() ?? this.Id;
        }
    }

    /// <summary>
    /// Encapsulates the data associated with the flow
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    public abstract class FlowContext
    {
        /// <summary>
        /// Gets or sets the unique flow context id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the tail context of the flow chain.
        /// </summary>
        public FlowContext Tail { get; set; }

        /// <summary>
        /// Gets or sets the error which might have happened during the flow execution.
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        /// Gets the Id of the head context of the flow chain
        /// </summary>
        /// <returns>Id of the flow chain head context</returns>
        public abstract Guid Head();
    }
}