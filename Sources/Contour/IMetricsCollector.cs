namespace Contour
{
    /// <summary>
    /// Component responsible for measuring performance indicators of the service bus
    /// </summary>
    public interface IMetricsCollector
    {
        /// <summary>
        /// Increases numeric performance indicator value by 1
        /// </summary>
        /// <param name="metricName">Indicator name</param>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="tags">Tags of value</param>
        void Increment(string metricName, double sampleRate = 1d, string[] tags = null);

        /// <summary>
        /// Decreases numeric performance indicator value by 1
        /// </summary>
        /// <param name="metricName">Indicator name</param>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="tags">Tags of value</param>
        void Decrement(string metricName, double sampleRate = 1d, string[] tags = null);

        /// <summary>
        /// Measures value of performance indicator for further aggregation
        /// </summary>
        /// <typeparam name="T">Type of <paramref name="value"/></typeparam>
        /// <param name="metricName">Indicator name</param>
        /// <param name="value">Measured value</param>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="tags">Tags of value</param>
        void Histogram<T>(string metricName, T value, double sampleRate = 1d, string[] tags = null);

        /// <summary>
        /// Sets performance indicator <paramref name="value"/>
        /// </summary>
        /// <typeparam name="T">Type of <paramref name="value"/></typeparam>
        /// <param name="metricName">Indicator name</param>
        /// <param name="value">Measured value</param>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="tags">Tags of value</param>
        void Gauge<T>(string metricName, T value, double sampleRate = 1.0, string[] tags = null);
    }
}