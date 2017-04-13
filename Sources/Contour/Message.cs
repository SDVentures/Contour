﻿namespace Contour
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///   Нестрого типизированный контейнер сообщения.
    /// </summary>
    public sealed class Message : IMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="label">
        /// The label.
        /// </param>
        /// <param name="headers">
        /// The headers.
        /// </param>
        /// <param name="payload">
        /// The payload.
        /// </param>
        public Message(MessageLabel label, IDictionary<string, object> headers, object payload)
        {
            this.Label = label;
            this.Headers = headers;
            this.Payload = payload;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="label">
        /// The label.
        /// </param>
        /// <param name="payload">
        /// The payload.
        /// </param>
        public Message(MessageLabel label, object payload)
            : this(label, new Dictionary<string, object>(), payload)
        {
        }

        /// <summary>
        /// Gets the headers.
        /// </summary>
        public IDictionary<string, object> Headers { get; private set; }

        /// <summary>
        ///   Метка сообщения.
        /// </summary>
        public MessageLabel Label { get; private set; }

        /// <summary>
        ///   Содержимое сообщения.
        /// </summary>
        public object Payload { get; private set; }

        /// <summary>
        /// Создает копию сообщения с указанной меткой.
        /// </summary>
        /// <param name="label">
        /// Новая метка сообщения.
        /// </param>
        /// <returns>
        /// Новое сообщение.
        /// </returns>
        public IMessage WithLabel(MessageLabel label)
        {
            return new Message(label, Headers, Payload);
        }

        /// <summary>
        /// Создает копию сообщения с указанным содержимым.
        /// </summary>
        /// <typeparam name="T">Тип содержимого.</typeparam>
        /// <param name="payload">Содержимое сообщения.</param>
        /// <returns>Новое сообщение.</returns>
        public IMessage WithPayload<T>(T payload) where T : class
        {
            return new Message(Label, Headers, payload);
        }
    }

    /// <summary>
    /// Строго типизированный контейнер сообщения.
    /// </summary>
    /// <typeparam name="T">
    /// Тип содержимого сообщения
    /// </typeparam>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    public sealed class Message<T> : IMessage
        where T : class
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Message{T}"/>.
        /// </summary>
        /// <param name="label">
        /// The label.
        /// </param>
        /// <param name="headers">
        /// The headers.
        /// </param>
        /// <param name="payload">
        /// The payload.
        /// </param>
        public Message(MessageLabel label, IDictionary<string, object> headers, T payload)
        {
            this.Label = label;
            this.Headers = headers;
            this.Payload = payload;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Message{T}"/>.
        /// </summary>
        /// <param name="label">
        /// The label.
        /// </param>
        /// <param name="payload">
        /// The payload.
        /// </param>
        public Message(MessageLabel label, T payload)
            : this(label, new Dictionary<string, object>(), payload)
        {
        }

        /// <summary>
        /// Gets the headers.
        /// </summary>
        public IDictionary<string, object> Headers { get; private set; }

        /// <summary>
        ///   Метка сообщения.
        /// </summary>
        public MessageLabel Label { get; private set; }

        /// <summary>
        ///   Содержимое сообщения.
        /// </summary>
        public T Payload { get; private set; }

        /// <summary>
        /// Gets the payload.
        /// </summary>
        object IMessage.Payload
        {
            get
            {
                return this.Payload;
            }
        }

        /// <summary>
        /// Создает копию сообщения с указанной меткой.
        /// </summary>
        /// <param name="label">
        /// Новая метка сообщения.
        /// </param>
        /// <returns>
        /// Новое сообщение.
        /// </returns>
        public IMessage WithLabel(MessageLabel label)
        {
            return new Message<T>(label, Headers, Payload);
        }

        /// <summary>
        /// Создает копию сообщения с указанным содержимым.
        /// </summary>
        /// <typeparam name="T1">Тип содержимого.</typeparam>
        /// <param name="payload">Содержимое сообщения.</param>
        /// <returns>Новое сообщение.</returns>
        public IMessage WithPayload<T1>(T1 payload) where T1 : class
        {
            return new Message<T1>(Label, Headers, payload);
        }
    }
}
