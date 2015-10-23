namespace Contour
{
    /// <summary>
    /// Конечная точка шины сообщений.
    /// Универсальный адаптер для передачи и получения сообщений из шины.
    /// </summary>
    internal class Endpoint : IEndpoint
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Endpoint"/>.
        /// </summary>
        /// <param name="address">Адрес конечной точки. Адрес должен быть уникальным для всех конечных точек шины сообщений.</param>
        public Endpoint(string address)
        {
            this.Address = address;
        }

        /// <summary>
        /// Адрес конечной точки. 
        /// </summary>
        public string Address { get; private set; }

        /// <summary>
        /// Сравнивает два объекта.
        /// </summary>
        /// <param name="obj">Сравниваемый объект.</param>
        /// <returns>
        /// <c>true</c> - если объекты равны, иначе <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((Endpoint)obj);
        }

        /// <summary>
        /// Вычисляет код хеширования конечной точки.
        /// </summary>
        /// <returns>Код хеширования конечной точки.</returns>
        public override int GetHashCode()
        {
            return this.Address != null ? this.Address.GetHashCode() : 0;
        }

        /// <summary>
        /// Формирует строковое представление конечной точки.
        /// </summary>
        /// <returns>Строковое представление конечной точки.</returns>
        public override string ToString()
        {
            return this.Address;
        }

        /// <summary>
        /// Сравнивает два объекта.
        /// </summary>
        /// <param name="other">Сравниваемый объект.</param>
        /// <returns>
        /// <c>true</c> - если объекты равны, иначе <c>false</c>.
        /// </returns>
        protected bool Equals(Endpoint other)
        {
            return string.Equals(this.Address, other.Address);
        }
    }
}
