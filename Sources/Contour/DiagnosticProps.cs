using System.Collections.Generic;
using System.Threading;

namespace Contour
{
    public static class DiagnosticProps
    {
        private static readonly AsyncLocal<Dictionary<string, string>> Storage = new AsyncLocal<Dictionary<string, string>>();

        internal static void Store(string name, string value)
        {
            if (Storage.Value == null)
            {
                Storage.Value = new Dictionary<string, string>();
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
        }
    }
}