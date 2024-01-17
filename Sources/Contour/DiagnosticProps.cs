using System.Collections.Concurrent;
using System.Threading;

namespace Contour
{
    public static class DiagnosticProps
    {
        private static readonly AsyncLocal<ConcurrentDictionary<string, string>> Storage = new AsyncLocal<ConcurrentDictionary<string, string>>();

        public static void Store(string name, string value)
        {
            if (Storage.Value == null)
            {
                Storage.Value = new ConcurrentDictionary<string, string>();
            }

            Storage.Value[name] = value;
        }

        public static string Get(string name)
        {
            if (Storage.Value == null)
            {
                return null;
            }

            return Storage.Value.ContainsKey(name) ? Storage.Value[name] : null;
        }

        public static class Names
        {
            public static readonly string LastPublishAttemptConnectionString = "LastPublishAttemptConnectionString";

            public static readonly string ConsumerConnectionString = "ConsumerConnectionString";

            public static readonly string Breadcrumbs = "Breadcrumbs";

            public static readonly string OriginalMessageId = "OriginalMessageId";
        }
    }
}
