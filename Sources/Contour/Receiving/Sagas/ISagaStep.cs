namespace Contour.Receiving.Sagas
{
    /// <summary>
    /// Обработчик шага саги.
    /// </summary>
    /// <typeparam name="TS">Тип состояния саги.</typeparam>
    /// <typeparam name="TM">Тип сообщения инициирующего шаг саги.</typeparam>
    /// <typeparam name="TK">Тип идентификатора саги.</typeparam>
    internal interface ISagaStep<TS, TM, TK>
        where TM : class
    {
        void Handle(ISagaContext<TS, TK> sagaContext, IConsumingContext<TM> consumingContext);
    }
}
