namespace Contour
{
    /// <summary>
    /// Хранилище ключ-значение.
    /// </summary>
    /// <typeparam name="T">Тип правил.</typeparam>
    public interface IKeyValueStorage<T> where T : class
    {
        /// <summary>
        /// Возвращает значение по ключу.
        /// </summary>
        /// <param name="key">Ключ, по которому получают значение..</param>
        /// <returns>Значение или <c>null</c>, если по ключу ничего не найдено.</returns>
        T Get(string key);

        /// <summary>
        /// Сохраняет значение с указанным ключом.
        /// </summary>
        /// <param name="key">Ключ значения.</param>
        /// <param name="value">Сохраняемое значение.</param>
        void Set(string key, T value);
    }
}
