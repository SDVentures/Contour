using System.Threading.Tasks;

namespace Contour.Flow.Configuration
{
    public interface IFlow
    {
        Task Completion { get; }
    }
}