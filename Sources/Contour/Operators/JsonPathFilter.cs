namespace Contour.Operators
{
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Фильтр на основе выражения JsonPath.
    /// </summary>
    public class JsonPathFilter : Filter
    {
        /// <summary>
        /// Инициализирует оператор.
        /// </summary>
        /// <param name="jsonPath">Выражение JsonPath.</param>
        public JsonPathFilter(string jsonPath)
            : base(m => Predicate(m, jsonPath))
        {
        }

        private static bool Predicate(IMessage message, string jsonPath)
        {
            var json = JObject.FromObject(message.Payload);
            return json.SelectToken(jsonPath) != null;
        }
    }
}
