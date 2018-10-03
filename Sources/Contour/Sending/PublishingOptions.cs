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
        private readonly IDictionary<string, Maybe> innerOptions = new Dictionary<string, Maybe>();

        #region Public Properties

        /// <summary>
        /// Gets or sets the persistently.
        /// </summary>
        public Maybe<bool> Persistently
        {
            get
            {
                return this.GetOption<bool>(Headers.Persist);
            }

            set
            {
                this.SetOption(Headers.Persist, value);
            }
        }

        /// <summary>
        /// Gets or sets the ttl.
        /// </summary>
        public Maybe<TimeSpan?> Ttl
        {
            get
            {
                return this.GetOption<TimeSpan?>(Headers.Ttl);
            }

            set
            {
                this.SetOption(Headers.Persist, value);
            }
        }

        /// <summary>
        /// Gets or sets the ttl.
        /// </summary>
        public Maybe<TimeSpan?> Delay
        {
            get
            {
                return this.GetOption<TimeSpan?>(Headers.Delay);
            }

            set
            {
                this.SetOption(Headers.Delay, value);
            }
        }

        #endregion

        public void SetOption<T>(string name, T value)
        {
            this.innerOptions[name] = (Maybe<T>)value;
        }

        public IDictionary<string, Maybe> GetAll()
        {
            return this.innerOptions;
        }

        private Maybe<T> GetOption<T>(string name)
        {
            return this.innerOptions.ContainsKey(Headers.Persist)
                ? (Maybe<T>)this.innerOptions[name]
                : Maybe<T>.Empty;
        }
    }
}
