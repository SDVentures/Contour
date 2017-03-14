namespace Contour.Flow.Configuration
{
    public interface IRequestResponseFlow<TInput, TOutput>: IFlowAccessor<TInput, TOutput>
    {
        IFlowRegistry Registry { set; }
        string Label { set; }
    }
}