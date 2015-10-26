using System;
using System.Collections.Generic;

namespace Contour
{
    /// <summary>
    ///   Сериализуемое описание исключения.
    /// </summary>
    public sealed class FaultException
    {
        private const int MaxNestingLevel = 5;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FaultException"/>. 
        /// Создает экземпляр описания исключения.
        /// </summary>
        /// <param name="exception">
        /// Исключение, по которому создается описание.
        /// </param>
        public FaultException(Exception exception)
            : this(exception, 0)
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FaultException"/>.
        /// </summary>
        /// <param name="exception">Исключение, по которому создается описание.</param>
        /// <param name="level">Текущий уровень вложенности исключения.</param>
        private FaultException(Exception exception, int level)
        {
            this.Message = exception.Message;
            this.Type = exception.GetType().FullName;
            this.StackTrace = exception.StackTrace;
            this.InnerExceptions = new List<FaultException>();

            if (level >= MaxNestingLevel)
            {
                return;
            }

            this.GetInnerExceptionsOf(exception, level);
        }

        /// <summary>
        ///   Сообщение исключения.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        ///   Стек исключения.
        /// </summary>
        public string StackTrace { get; private set; }

        /// <summary>
        ///   Тип исключения.
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// Коллекция исключений, которые стали источником корневого исключения.
        /// </summary>
        public IList<FaultException> InnerExceptions { get; private set; }

        /// <summary>
        /// Получает внутренние исключения и конвертирует их в описания.
        /// </summary>
        /// <param name="exception">Конвертируемое исключение.</param>
        /// <param name="level">Текущий уровень вложенности исключения.</param>
        private void GetInnerExceptionsOf(Exception exception, int level)
        {
            var aggregateException = exception as AggregateException;
            if (aggregateException != null && aggregateException.InnerExceptions != null)
            {
                foreach (var innerExceptin in aggregateException.InnerExceptions)
                {
                    this.InnerExceptions.Add(new FaultException(innerExceptin, level + 1));
                }
            }
            else if (exception.InnerException != null)
            {
                this.InnerExceptions.Add(new FaultException(exception.InnerException, level + 1));
            }
        }
    }
}
