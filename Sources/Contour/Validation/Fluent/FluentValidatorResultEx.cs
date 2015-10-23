// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FluentValidatorResultEx.cs" company="">
//   
// </copyright>
// <summary>
//   The fluent validator result ex.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Validation.Fluent
{
    using System.Linq;

    /// <summary>
    /// The fluent validator result ex.
    /// </summary>
    internal static class FluentValidatorResultEx
    {
        #region Public Methods and Operators

        /// <summary>
        /// The to native.
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <returns>
        /// The <see cref="ValidationResult"/>.
        /// </returns>
        public static ValidationResult ToNative(this FluentValidation.Results.ValidationResult result)
        {
            return result.IsValid ? ValidationResult.Valid : new ValidationResult(result.Errors.Select(e => new BrokenRule(e.ErrorMessage)));
        }

        #endregion
    }
}
