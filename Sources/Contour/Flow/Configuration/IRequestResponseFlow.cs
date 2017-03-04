namespace Contour.Flow.Configuration
{
    public interface IRequestResponseFlow<in TInput, out TOutput>: IFlowAccessor<TInput, TOutput>
    {
        IFlowRegistry Registry { set; }
        string Label { set; }
    }
}