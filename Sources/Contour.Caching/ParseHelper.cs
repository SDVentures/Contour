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
    }
}