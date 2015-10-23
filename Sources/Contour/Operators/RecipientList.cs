using System;
using System.Collections.Generic;
using System.Linq;

namespace Contour.Operators
{
    /// <summary>
    /// Оператор, который перенаправляет запросы динамически определяемым получателям (реализация шаблона <see href="http://www.eaipatterns.com/RecipientList.html"/>).
    /// Оператор анализирует входящее сообщение, определяет список нужных получателей и перенаправляет сообщения им.
    /// </summary>
    public class RecipientList : IMessageOperator
    {
        private readonly Func<IMessage, MessageLabel[]> determineRecipientList;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RecipientList"/>.
        /// </summary>
        /// <param name="determineRecipientList">Функция определения списка получателей сообщения.</param>
        public RecipientList(Func<IMessage, MessageLabel[]> determineRecipientList)
        {
            this.determineRecipientList = determineRecipientList;
        }

        /// <summary>
        /// Инспектирует входящее сообщение, определяет список получателей и передает сообщение всем получателям.
        /// </summary>
        /// <param name="message">Входящее сообщение.</param>
        /// <returns>Сообщения для получателей, определенных на основе содержимого входящего сообщения.</returns>
        public virtual IEnumerable<IMessage> Apply(IMessage message)
        {
            return this.determineRecipientList(message).Select(message.WithLabel);
        }
    }
}
