using System;

using Newtonsoft.Json;

namespace Contour.Helpers
{
    /// <summary>
    /// Рассчитывает значение хеш-функции для содержимого.
    /// </summary>
    public class Hasher
    {
        /// <summary>
        /// Вычисляет значение хеш-функции для объекта.
        /// </summary>
        /// <param name="obj">Объект, для которого вычисляется значение хеш-функции.</param>
        /// <returns>Вычисленный значение хеш-функции.</returns>
        [Obsolete("Необходимо использовать метод рассчета значения хеш-функции на основе входящего сообщения (IMessage).")]
        public long CalculateHashOf(object obj)
        {
            return JsonConvert.SerializeObject(obj).GetHashCode();
        }

        /// <summary>
        /// Вычисляет значение хеш-функции для сообщения.
        /// </summary>
        /// <param name="message">Сообщение, для которого вычисляется значение хеш-функции.</param>
        /// <returns>Вычисленный значение хеш-функции.</returns>
        public long CalculateHashOf(IMessage message)
        {
            unchecked 
            {         
                int hash = 27;
                hash = (13 * hash) + JsonConvert.SerializeObject(message.Payload).GetHashCode();
                hash = (13 * hash) + message.Label.GetHashCode();
                return hash;
            }
        }
    }
}
