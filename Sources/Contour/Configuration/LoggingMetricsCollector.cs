using Common.Logging;

namespace Contour.Configuration
{
    internal class LoggingMetricsCollector : IMetricsCollector
    {
        private static readonly ILog Log = LogManager.GetLogger<LoggingMetricsCollector>();

        public void Increment(string metricName, double sampleRate = 1, string[] tags = null)
        {
            Log.Trace(m => m("Incremented {0} with sample rate {1} and tags [{2}]", metricName, sampleRate, LogTags(tags)));
        }

        public void Decrement(string metricName, double sampleRate = 1, string[] tags = null)
        {
            Log.Trace(m => m("Decremented {0} with sample rate {1} and tags [{2}]", metricName, sampleRate, LogTags(tags)));
        }

        public void Histogram<T>(string metricName, T value, double sampleRate = 1, string[] tags = null)
        {
            Log.Trace(m => m("Collected histogram {0} value {3} with sample rate {1} and tags [{2}]", metricName, sampleRate, LogTags(tags), value));
        }

        public void Gauge<T>(string metricName, T value, double sampleRate = 1, string[] tags = null)
        {
            Log.Trace(m => m("Collected gauge {0} value {3} with sample rate {1} and tags [{2}]", metricName, sampleRate, LogTags(tags), value));
        }

        private static string LogTags(string[] tags) => tags != null ? string.Join(", ", tags) : "null";
    }
}