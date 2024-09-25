// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublishingOptions.cs" company="">
//   
// </copyright>
// <summary>
//   The publishing options.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Contour.Sending
{
    using System;

    using Contour.Helpers;

    /// <summary>
    /// The publishing options.
    /// </summary>
    public class PublishingOptions
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the persistently.
        /// </summary>
        public Maybe<bool> Persistently { get; set; }

        /// <summary>
        /// Gets or sets the ttl.
        /// </summary>
        public Maybe<TimeSpan?> Ttl { get; set; }

        /// <summary>
        /// Gets or sets the ttl.
        /// </summary>
        public TimeSpan? Delay { get; set; }
        
        public string DirectId { get; set; }

        /// <summary>
        /// Дополнительные заголовки
        /// </summary>
        public Dictionary<string, object> AdditionalHeaders { get; set; }

        /// <summary>
        /// Префикс заголовка x-breadcrumbs
        /// </summary>
        public string BreadcrumbsPrefix { get; set; }

        #endregion
    }
}
