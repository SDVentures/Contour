using System.Collections.Generic;
using System.Threading;

namespace Contour
{
    public static class MetricProps
    {
        private static readonly AsyncLocal<Dictionary<string, string>> Storage = new AsyncLocal<Dictionary<string, string>>();

        public static void Store(string name, string value)
        {
            Storage.Value[name] = value;
        }

        public static string Get(string name)
        {
            return Storage.Value.ContainsKey(name) ? Storage.Value[name] : null;
        }

        public static class Names
        {
            public static readonly string LastPublishAttemptConnectionString = "LastPublishAttemptConnectionString";
        }
    }
}