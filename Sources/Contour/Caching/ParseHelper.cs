using System;

namespace Contour.Caching
{
    using System.Collections.Generic;

    public static class ParseHelper
    {
        public delegate bool SafeParser<T>(string input, out T parsed);

        public static T GetParsedValue<T>(IDictionary<string, string> dictionary, string key, SafeParser<T> parser, T defaultValue = default(T))
        {
            T parsed;
            return dictionary.ContainsKey(key) && parser(dictionary[key], out parsed) ? parsed : defaultValue;
        }

        public static bool TryParseNullableTimeSpan(string input, out TimeSpan? parsed)
        {
            TimeSpan timespan;
            if (TimeSpan.TryParse(input, out timespan))
            {
                parsed = timespan;
                return true;
            }

            parsed = null;
            return false;
        }
    }
}