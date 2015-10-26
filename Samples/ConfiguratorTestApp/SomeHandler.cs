// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SomeHandler.cs" company="">
//   
// </copyright>
// <summary>
//   The some handler.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ConfiguratorTestApp
{
    using System.Dynamic;

    using Common.Logging;

    using Contour.Receiving;
    using Contour.Receiving.Consumers;

    /// <summary>
    /// The some handler.
    /// </summary>
    public class SomeHandler : IConsumerOf<ExpandoObject>
    {
        #region Static Fields

        /// <summary>
        /// The _logger.
        /// </summary>
        private static readonly ILog _logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public void Handle(IConsumingContext<ExpandoObject> context)
        {
            _logger.InfoFormat("Received [{0}].", context.Message.Label);
        }

        #endregion
    }
}
