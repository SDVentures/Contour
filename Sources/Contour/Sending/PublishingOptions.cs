using System;
using System.Collections.Generic;

using Contour.Helpers;

namespace Contour.Sending
{
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

        /// <summary>
        /// Sets an option with custom name.
        /// </summary>
        /// <typeparam name="T">Option value type.</typeparam>
        /// <param name="name">Name of the option.</param>
        /// <param name="value">Option value.</param>
        public void SetOption<T>(string name, T value)
        {
            this.innerOptions[name] = (Maybe<T>)value;
        }

        /// <summary>
        /// Gets an option value by name.
        /// </summary>
        /// <typeparam name="T">Option value type.</typeparam>
        /// <param name="name">Option name.</param>
        /// <returns>Maybe option value.</returns>
        public Maybe<T> GetOption<T>(string name)
        {
            return this.innerOptions.ContainsKey(Headers.Persist) ? (Maybe<T>)this.innerOptions[name] : Maybe<T>.Empty;
        }

        /// <summary>
        /// Returns all options as a dictionary, where keys are names.
        /// </summary>
        /// <returns>Options dictionary, where keys are names.</returns>
        public virtual IDictionary<string, Maybe> GetAll()
        {
            return this.innerOptions;
        }
    }
}
