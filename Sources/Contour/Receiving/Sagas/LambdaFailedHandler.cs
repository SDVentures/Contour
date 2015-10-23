using System;

namespace Contour.Receiving.Sagas
{
    /// <summary>
    /// Обрабатывает ошибки возникающие при выполнении шага саги.
    /// </summary>
    /// <typeparam name="TS">Тип данных саги.</typeparam>
    /// <typeparam name="TM">Тип входящего сообщения.</typeparam>
    /// <typeparam name="TK">Тип идентификатора саги.</typeparam>
    internal class LambdaFailedHandler<TS, TM, TK> : ISagaFailedHandler<TS, TM, TK>
        where TM : class
    {
        private readonly Action<IConsumingContext<TM>> notFoundAction;

        private readonly Action<ISagaContext<TS, TK>, IConsumingContext<TM>, Exception> failedAction;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="LambdaFailedHandler{TS,TM,TK}"/>. 
        /// </summary>
        /// <param name="notFoundAction">Обработчик ситуации, когда сага не найдена по идентификатору.</param>
        /// <param name="failedAction">Обработчик ситуации, когда при выполнении шага саги произошла ошибка.</param>
        public LambdaFailedHandler(Action<IConsumingContext<TM>> notFoundAction, Action<ISagaContext<TS, TK>, IConsumingContext<TM>, Exception> failedAction)
        {
            this.notFoundAction = notFoundAction;
            this.failedAction = failedAction;
        }

        /// <summary>
        /// Обрабатывает ситуацию, когда сага не найдена.
        /// </summary>
        /// <param name="context">Контекст обработки сообщения, в котором возникла эта ситуация.</param>
        public void SagaNotFoundHandle(IConsumingContext<TM> context)
        {
            this.notFoundAction(context);
        }

        /// <summary>
        /// Обрабатывает исключения возникшие при выполнении шага саги.
        /// </summary>
        /// <param name="sagaContext">Данные саги.</param>
        /// <param name="context">Контекст полученного сообщения.</param>
        /// <param name="exception">Обрабатываемое исключение.</param>
        public void SagaFailedHandle(ISagaContext<TS, TK> sagaContext, IConsumingContext<TM> context, Exception exception)
        {
            this.failedAction(sagaContext, context, exception);
        }
    }
}
