namespace Contour
{
    public interface IMetricsCollector
    {
        void Increment(string metricName, double sampleRate = 1d, string[] tags = null);

        void Decrement(string metricName, double sampleRate = 1d, string[] tags = null);

        void Histogram<T>(string metricName, T value, double sampleRate = 1d, string[] tags = null);

        void Gauge<T>(string metricName, T value, double sampleRate = 1.0, string[] tags = null);
    }
}