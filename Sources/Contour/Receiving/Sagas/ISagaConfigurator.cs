using System;

namespace Contour.Receiving.Sagas
{
    /// <summary>
    /// Конфигурация саги.
    /// </summary>
    /// <typeparam name="TS">Тип пользовательских данных сохраняемых в саге.</typeparam>
    /// <typeparam name="TM">Тип входящего сообщения.</typeparam>
    /// <typeparam name="TK">Тип идентификатора саги.</typeparam>
    internal interface ISagaConfigurator<TS, TM, TK>
        where TM : class
    {
        /// <summary>
        /// Регистрирует новый обработчик входящего сообщения вызываемый для выполнения шага саги.
        /// </summary>
        /// <param name="action">Обработчик входящего сообщения вызываемый для выполнения шага саги.</param>
        /// <returns>Конфигурация саги с зарегистрированным обработчиком входящего сообщения.</returns>
        ISagaConfigurator<TS, TM, TK> ReactWith(Action<ISagaContext<TS, TK>, IConsumingContext<TM>> action);

        /// <summary>
        /// Регистрирует новый обработчик входящего сообщения вызываемый для выполнения шага саги.
        /// </summary>
        /// <param name="canInitate">Верно, если входящее сообщение может создать новую сагу.</param>
        /// <param name="action">Обработчик входящего сообщения вызываемый для выполнения шага саги.</param>
        /// <returns>Конфигурация саги с зарегистрированным обработчиком входящего сообщения.</returns>
        ISagaConfigurator<TS, TM, TK> ReactWith(bool canInitate, Action<ISagaContext<TS, TK>, IConsumingContext<TM>> action);

        /// <summary>
        /// Регистрирует шаг саги выполняемый при обработке входящего сообщения.
        /// </summary>
        /// <param name="sagaStep">Обработчик входящего сообщения вызываемый для выполнения шага саги.</param>
        /// <returns>Конфигурация саги с зарегистрированным обработчиком входящего сообщения.</returns>
        ISagaConfigurator<TS, TM, TK> ReactWith(ISagaStep<TS, TM, TK> sagaStep);

        /// <summary>
        /// Регистрирует новый обработчик входящего сообщения вызываемый для выполнения шага саги.
        /// </summary>
        /// <param name="canInitate">Верно, если входящее сообщение может создать новую сагу.</param>
        /// <param name="sagaStep">Выполняемый шаг саги, который вызывается при обработке входящего сообщения.</param>
        /// <returns>Конфигурация саги с зарегистрированным обработчиком входящего сообщения.</returns>
        ISagaConfigurator<TS, TM, TK> ReactWith(bool canInitate, ISagaStep<TS, TM, TK> sagaStep);

        /// <summary>
        /// Регистрирует обработчик ошибок, которые возникают при выполнении шага саги.
        /// </summary>
        /// <param name="sagaFailedHandler">Обработчик ошибок.</param>
        /// <returns>Конфигурация саги с зарегистрированным обработчиком ошибок.</returns>
        ISagaConfigurator<TS, TM, TK> OnSagaFailed(ISagaFailedHandler<TS, TM, TK> sagaFailedHandler);

        /// <summary>
        /// Регистрирует обработчики ошибок, которые возникают при выполнении шага саги.
        /// </summary>
        /// <param name="notFoundHandler">Обработчик ситуации, когда сага не найдена.</param>
        /// <param name="failedAction">Обработчик ситуации, когда при выполнении шага саги произошло исключение.</param>
        /// <returns>Конфигурация саги с зарегистрированным обработчиком ошибок.</returns>
        ISagaConfigurator<TS, TM, TK> OnSagaFailed(Action<IConsumingContext<TM>> notFoundHandler, Action<ISagaContext<TS, TK>, IConsumingContext<TM>, Exception> failedAction);

        /// <summary>
        /// Регистрирует жизненный цикл саги.
        /// </summary>
        /// <param name="sagaLifecycle">Жизненный цикл саги.</param>
        /// <returns>Конфигурация саги с зарегистрированным жизненным циклом саги.</returns>
        ISagaConfigurator<TS, TM, TK> UseLifeCycle(ISagaLifecycle<TS, TM, TK> sagaLifecycle);

        /// <summary>
        /// Регистрирует фабрику саги.
        /// </summary>
        /// <param name="sagaFactory">Фабрика саги.</param>
        /// <returns>Конфигурация саги с зарегистрированным жизненным циклом саги.</returns>
        ISagaConfigurator<TS, TM, TK> UseSagaFactory(ISagaFactory<TS, TK> sagaFactory);

        /// <summary>
        /// Регистрирует методы фабрики саги.
        /// </summary>
        /// <param name="factoryById">Делегат создающий сагу на основе ее идентификатора.</param>
        /// <param name="factoryByData">Делегат создающий сагу на основе ее идентификатора и пользовательских данных.</param>
        /// <returns>Конфигурация саги с зарегистрированной фабрикой саги.</returns>
        ISagaConfigurator<TS, TM, TK> UseSagaFactory(
            Func<TK, ISagaContext<TS, TK>> factoryById, 
            Func<TK, TS, ISagaContext<TS, TK>> factoryByData);

        /// <summary>
        /// Регистрирует хранилище саги.
        /// </summary>
        /// <param name="sagaRepository">Хранилище саги.</param>
        /// <returns>Конфигурация саги с зарегистрированной фабрикой саги.</returns>
        ISagaConfigurator<TS, TM, TK> UseSagaRepository(ISagaRepository<TS, TK> sagaRepository);

        /// <summary>
        /// Регистрирует вычислитель идентификатора саги.
        /// </summary>
        /// <param name="sagaIdSeparator">Вычислитель идентификатора саги.</param>
        /// <returns>Конфигурация саги с зарегистрированным вычислителем идентификатора.</returns>
        ISagaConfigurator<TS, TM, TK> UseSagaIdSeparator(ISagaIdSeparator<TM, TK> sagaIdSeparator);

        /// <summary>
        /// Регистрирует вычислитель идентификатора саги.
        /// </summary>
        /// <param name="separator">Вычислитель идентификатора саги.</param>
        /// <returns>Конфигурация саги с зарегистрированным вычислителем идентификатора.</returns>
        ISagaConfigurator<TS, TM, TK> UseSagaIdSeparator(Func<Message<TM>, TK> separator);

        /// <summary>
        /// Возвращает конфигурацию получателя входящего сообщения.
        /// </summary>
        /// <returns>Конфигурация получателя входящего сообщения.</returns>
        IReceiverConfigurator<TM> AsReceiver();
    }
}
