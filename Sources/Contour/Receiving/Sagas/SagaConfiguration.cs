using System;

namespace Contour.Receiving.Sagas
{
    /// <summary>
    /// Конфигурация саги.
    /// </summary>
    /// <typeparam name="TS">Тип пользовательских данных сохраняемых в саге.</typeparam>
    /// <typeparam name="TM">Тип входящего сообщения.</typeparam>
    /// <typeparam name="TK">Тип идентификатора саги.</typeparam>
    internal class SagaConfiguration<TS, TM, TK> : ISagaConfigurator<TS, TM, TK>
        where TM : class
    {
        private readonly IReceiverConfigurator<TM> receiverConfigurator;

        private SagaConsumerOf<TS, TM, TK> sagaConsumer;

        private DefaultSagaLifecycle<TS, TM, TK> sagaLifecycle;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SagaConfiguration{TS,TM,TK}"/>. 
        /// </summary>
        /// <param name="receiverConfigurator">Конфигурация получателя входящего сообщения.</param>
        /// <param name="sagaRepository">Хранилище саги.</param>
        /// <param name="sagaIdSeparator">Вычислитель идентификатора саги.</param>
        /// <param name="sagaFactory">Фабрика саги.</param>
        /// <param name="sagaStep">Действие выполняемое при обработке саги.</param>
        /// <param name="sagaFailedHandler">Обработчик возникающих ошибок.</param>
        public SagaConfiguration(
            IReceiverConfigurator<TM> receiverConfigurator,
            ISagaRepository<TS, TK> sagaRepository,
            ISagaIdSeparator<TM, TK> sagaIdSeparator,
            ISagaFactory<TS, TK> sagaFactory,
            ISagaStep<TS, TM, TK> sagaStep,
            ISagaFailedHandler<TS, TM, TK> sagaFailedHandler)
        {
            this.receiverConfigurator = receiverConfigurator;

            this.sagaLifecycle = new DefaultSagaLifecycle<TS, TM, TK>(sagaRepository, sagaIdSeparator, sagaFactory);
            this.sagaConsumer = new SagaConsumerOf<TS, TM, TK>(this.sagaLifecycle, sagaStep, false, sagaFailedHandler);
        }

        /// <inheritdoc />
        public ISagaConfigurator<TS, TM, TK> ReactWith(Action<ISagaContext<TS, TK>, IConsumingContext<TM>> action)
        {
            this.sagaConsumer.SagaStep = new LambdaSagaStep<TS, TM, TK>(action);
            this.receiverConfigurator.ReactWith(() => this.sagaConsumer);

            return this;
        }

        /// <inheritdoc />
        public ISagaConfigurator<TS, TM, TK> ReactWith(bool canInitate, Action<ISagaContext<TS, TK>, IConsumingContext<TM>> action)
        {
            this.sagaConsumer.SagaStep = new LambdaSagaStep<TS, TM, TK>(action);
            this.sagaConsumer.CanInitiate = canInitate;
            this.receiverConfigurator.ReactWith(() => this.sagaConsumer);

            return this;
        }

        /// <inheritdoc />
        public ISagaConfigurator<TS, TM, TK> ReactWith(ISagaStep<TS, TM, TK> sagaStep)
        {
            this.sagaConsumer.SagaStep = sagaStep;
            this.receiverConfigurator.ReactWith(() => this.sagaConsumer);

            return this;
        }

        /// <inheritdoc />
        public ISagaConfigurator<TS, TM, TK> ReactWith(bool canInitate, ISagaStep<TS, TM, TK> sagaStep)
        {
            this.sagaConsumer.SagaStep = sagaStep;
            this.sagaConsumer.CanInitiate = canInitate;
            this.receiverConfigurator.ReactWith(() => this.sagaConsumer);

            return this;
        }

        /// <inheritdoc />
        public ISagaConfigurator<TS, TM, TK> OnSagaFailed(ISagaFailedHandler<TS, TM, TK> sagaFailedHandler)
        {
            this.sagaConsumer.SagaFailedHandler = sagaFailedHandler;

            return this;
        }

        /// <inheritdoc />
        public ISagaConfigurator<TS, TM, TK> OnSagaFailed(Action<IConsumingContext<TM>> notFoundHandler, Action<ISagaContext<TS, TK>, IConsumingContext<TM>, Exception> failedAction)
        {
            this.sagaConsumer.SagaFailedHandler = new LambdaFailedHandler<TS, TM, TK>(notFoundHandler, failedAction);

            return this;
        }

        /// <inheritdoc />
        public ISagaConfigurator<TS, TM, TK> UseLifeCycle(ISagaLifecycle<TS, TM, TK> sagaLifecycle)
        {
            this.sagaConsumer.SagaLifecycle = sagaLifecycle;

            return this;
        }

        /// <inheritdoc />
        public ISagaConfigurator<TS, TM, TK> UseSagaFactory(ISagaFactory<TS, TK> sagaFactory)
        {
            this.sagaLifecycle.SagaFactory = sagaFactory;

            return this;
        }

        /// <inheritdoc />
        public ISagaConfigurator<TS, TM, TK> UseSagaFactory(Func<TK, ISagaContext<TS, TK>> factoryById, Func<TK, TS, ISagaContext<TS, TK>> factoryByData)
        {
            this.sagaLifecycle.SagaFactory = new LambdaSagaFactory<TS, TK>(factoryById, factoryByData);

            return this;
        }

        /// <inheritdoc />
        public ISagaConfigurator<TS, TM, TK> UseSagaRepository(ISagaRepository<TS, TK> sagaRepository)
        {
            this.sagaLifecycle.SagaRepository = sagaRepository;

            return this;
        }

        /// <inheritdoc />
        public ISagaConfigurator<TS, TM, TK> UseSagaIdSeparator(ISagaIdSeparator<TM, TK> sagaIdSeparator)
        {
            this.sagaLifecycle.SagaIdSeparator = sagaIdSeparator;

            return this;
        }

        /// <inheritdoc />
        public ISagaConfigurator<TS, TM, TK> UseSagaIdSeparator(Func<Message<TM>, TK> separator)
        {
            this.sagaLifecycle.SagaIdSeparator = new LambdaSagaSeparator<TM, TK>(separator);

            return this;
        }

        /// <inheritdoc />
        public IReceiverConfigurator<TM> AsReceiver()
        {
            return this.receiverConfigurator;
        }
    }
}
