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
        public Maybe<TimeSpan> Delay
        {
            get
            {
                var longValue = this.GetOption<long>(Headers.Delay);

                return longValue.HasValue 
                           ? TimeSpan.FromMilliseconds(longValue.Value)
                           : Maybe<TimeSpan>.Empty;
            }

            set
            {
                if (!value.HasValue)
                {
                    return;
                }

                var mseconds = (long)value.Value.TotalMilliseconds;

                this.SetOption(Headers.Delay, (Maybe<long>)mseconds);
            }
        }

        #endregion

        /// <summary>
        /// Sets an option with custom name.
        /// </summary>
        /// <typeparam name="T">Option value type.</typeparam>
        /// <param name="name">Name of the option.</param>
        /// <param name="value">Option value.</param>
        public void SetOption<T>(string name, T value) where T : Maybe
        {
            this.innerOptions[name] = value;
        }

        /// <summary>
        /// Gets an option value by name.
        /// </summary>
        /// <typeparam name="T">Option value type.</typeparam>
        /// <param name="name">Option name.</param>
        /// <returns>Maybe option value.</returns>
        public Maybe<T> GetOption<T>(string name)
        {
            return this.innerOptions.ContainsKey(name) 
                       ? (Maybe<T>)this.innerOptions[name] 
                       : Maybe<T>.Empty;
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
