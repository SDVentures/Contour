﻿namespace Contour.Helpers
{
    using System;

    /// <summary>
    /// The disposable ex.
    /// </summary>
    internal static class DisposableEx
    {
        /// <summary>
        /// The try dispose.
        /// </summary>
        /// <param name="disposable">
        /// The disposable.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool TryDispose(this IDisposable disposable)
        {
            try
            {
                disposable.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
