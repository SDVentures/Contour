using System;

namespace Contour.Receiving.Sagas
{
    /// <summary>
    /// Обработчик ошибок саги.
    /// </summary>
    /// <typeparam name="TS">Тип состояния саги.</typeparam>
    /// <typeparam name="TM">Тип сообщения.</typeparam>
    /// <typeparam name="TK">Тип идентификатора саги.</typeparam>
    internal interface ISagaFailedHandler<TS, TM, TK>
        where TM : class
    {
        /// <summary>
        /// Обрабатывает ситуацию, когда сага не найдена.
        /// </summary>
        /// <param name="context">Контекст обработки сообщения, в котором возникла эта ситуация.</param>
        void SagaNotFoundHandle(IConsumingContext<TM> context);

        /// <summary>
        /// Обрабатывает исключения возникшие при выполнении шага саги.
        /// </summary>
        /// <param name="sagaContext">Данные саги.</param>
        /// <param name="context">Контекст полученного сообщения.</param>
        /// <param name="exception">Обрабатываемое исключение.</param>
        void SagaFailedHandle(ISagaContext<TS, TK> sagaContext, IConsumingContext<TM> context, Exception exception);
    }
}
