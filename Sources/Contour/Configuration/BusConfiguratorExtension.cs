using System;

using Contour.Receiving.Sagas;

namespace Contour.Configuration
{
    /// <summary>
    /// Класс расширения интерфейса <see cref="IBusConfigurator"/>.
    /// Нужен для того, чтобы скрыть неготовые для публикации методы, но дать возможность работать с ними внутри библиотеки.
    /// Для публикации методы переносятся в интерфейс <see cref="IBusConfigurator"/> и его реализации.
    /// </summary>
    internal static class BusConfiguratorExtension
    {
        public static ISagaConfigurator<TS, TM, TK> AsSagaStep<TS, TM, TK>(this IBusConfigurator busConfigurator, MessageLabel label) where TM : class
        {
            var receiverConfigurator = busConfigurator.On<TM>(label);

            return new SagaConfiguration<TS, TM, TK>(
                receiverConfigurator,
                SingletonSagaRepository<TS, TK>.Instance,
                new CorrelationIdSeparator<TM, TK>(),
                SingletonSagaFactory<TS, TK>.Instance, 
                new LambdaSagaStep<TS, TM, TK>((sc, cc) => { }), 
                new LambdaFailedHandler<TS, TM, TK>(cc => { throw new ArgumentException("Sage not found"); }, (sc, cc, e) => { }));
        }
    }
}
