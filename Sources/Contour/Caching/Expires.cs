namespace Contour.Caching
{
    using System;
    using System.Globalization;

    using Contour.Helpers.CodeContracts;

    /// <summary>
    /// The expires.
    /// </summary>
    public class Expires
    {
        /// <summary>
        /// The date prefix.
        /// </summary>
        private const string DatePrefix = "at";

        /// <summary>
        /// The period prefix.
        /// </summary>
        private const string PeriodPrefix = "in";
        /// <summary>
        /// The date.
        /// </summary>
        public readonly DateTimeOffset? Date;

        /// <summary>
        /// The period.
        /// </summary>
        public readonly TimeSpan? Period;
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Expires"/>.
        /// </summary>
        /// <param name="period">
        /// The period.
        /// </param>
        private Expires(TimeSpan period)
        {
            this.Period = period;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Expires"/>.
        /// </summary>
        /// <param name="date">
        /// The date.
        /// </param>
        private Expires(DateTimeOffset date)
        {
            this.Date = date;
        }
        /// <summary>
        /// The at.
        /// </summary>
        /// <param name="date">
        /// The date.
        /// </param>
        /// <returns>
        /// The <see cref="Expires"/>.
        /// </returns>
        public static Expires At(DateTimeOffset date)
        {
            return new Expires(date);
        }

        /// <summary>
        /// The in.
        /// </summary>
        /// <param name="period">
        /// The period.
        /// </param>
        /// <returns>
        /// The <see cref="Expires"/>.
        /// </returns>
        public static Expires In(TimeSpan period)
        {
            return new Expires(period);
        }

        /// <summary>
        /// The parse.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The <see cref="Expires"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        public static Expires Parse(string input)
        {
            string[] parts = input.Split(' ');
            Requires.True(parts.Length == 2, "s", "Invalid expiration data value [{0}].", input);

            if (parts[0] == PeriodPrefix)
            {
                return new Expires(TimeSpan.FromSeconds(uint.Parse(parts[1])));
            }

            if (parts[0] == DatePrefix)
            {
                return new Expires(
                    DateTimeOffset.Parse(parts[1], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).
                        ToLocalTime());
            }

            throw new ArgumentException("Unable to parse expiration from [{0}].".FormatEx(input), "input");
        }

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        public override string ToString()
        {
            if (this.Period.HasValue)
            {
                return PeriodPrefix + " " + this.Period.Value.TotalSeconds;
            }

            if (this.Date.HasValue)
            {
                return DatePrefix + " " + this.Date.Value.ToUniversalTime().
                                              ToString("s");
            }

            throw new InvalidOperationException("Invalid expiration specification.");
        }
    }
}
