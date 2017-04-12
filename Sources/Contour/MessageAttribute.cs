namespace Contour
{
    using System;

    /// <summary>
    /// The message attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageAttribute : Attribute
    {
        /// <summary>
        /// The label.
        /// </summary>
        public readonly string Label;
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MessageAttribute"/>.
        /// </summary>
        /// <param name="label">
        /// The label.
        /// </param>
        public MessageAttribute(string label)
        {
            this.Label = label;
        }
    }
}
