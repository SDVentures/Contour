namespace Contour
{
    using System.Text.RegularExpressions;

    /// <summary>
    ///   Метка сообщения.
    /// </summary>
    public sealed class MessageLabel
    {
        #region Constants

        /// <summary>
        /// The alias prefix.
        /// </summary>
        internal const string AliasPrefix = ":";

        /// <summary>
        /// The any name.
        /// </summary>
        private const string AnyName = "*";

        /// <summary>
        /// The empty name.
        /// </summary>
        private const string EmptyName = "";

        #endregion

        #region Static Fields

        /// <summary>
        /// The alias validator.
        /// </summary>
        private static readonly Regex AliasValidator = new Regex(@"\A^:[a-zA-Z_][a-zA-Z\d-_.:]*\z", RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// The label validator.
        /// </summary>
        private static readonly Regex LabelValidator = new Regex(@"\A^[a-zA-Z_*][a-zA-Z\d-_.:]*\z", RegexOptions.Singleline | RegexOptions.Compiled);

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MessageLabel"/>.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        private MessageLabel(string name)
        {
            this.Name = (name ?? EmptyName).ToLower();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///   Метка без имени. Используется там, где метка ожидается, но имя не важно.
        /// </summary>
        public static MessageLabel Any
        {
            get
            {
                return new MessageLabel(AnyName);
            }
        }

        /// <summary>
        ///   Пустая метка.
        /// </summary>
        public static MessageLabel Empty
        {
            get
            {
                return new MessageLabel(EmptyName);
            }
        }

        /// <summary>
        ///   Показывает, является ли метка псевдонимом.
        /// </summary>
        public bool IsAlias
        {
            get
            {
                return this.Name.StartsWith(AliasPrefix);
            }
        }

        /// <summary>
        ///   Показывает, привязана ли метка к конкретному имени.
        /// </summary>
        public bool IsAny
        {
            get
            {
                return this.Equals(Any);
            }
        }

        /// <summary>
        ///   Показывает, является ли метка пустой (отсутствующей).
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return this.Equals(Empty);
            }
        }

        /// <summary>
        ///   Название метки.
        /// </summary>
        public string Name { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Создает экземпляр метки сообщения.
        /// </summary>
        /// <param name="name">
        /// Название метки.
        /// </param>
        /// <returns>
        /// The <see cref="MessageLabel"/>.
        /// </returns>
        public static MessageLabel From(string name)
        {
            return new MessageLabel(name);
        }

        /// <summary>
        /// The is valid alias.
        /// </summary>
        /// <param name="alias">
        /// The alias.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsValidAlias(string alias)
        {
            return AliasValidator.IsMatch(alias);
        }

        /// <summary>
        /// The is valid label.
        /// </summary>
        /// <param name="label">
        /// The label.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsValidLabel(string label)
        {
            if (label.Equals(EmptyName))
            {
                return true;
            }

            return LabelValidator.IsMatch(label);
        }

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
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

            return obj is MessageLabel && this.Equals((MessageLabel)obj);
        }

        /// <summary>
        /// The get hash code.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return this.Name != null ? this.Name.GetHashCode() : 0;
        }

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="other">
        /// The other.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool Equals(MessageLabel other)
        {
            return string.Equals(this.Name, other.Name);
        }

        #endregion
    }
}
