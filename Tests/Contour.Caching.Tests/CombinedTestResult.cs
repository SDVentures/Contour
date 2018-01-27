namespace Contour.Caching.Tests
{
    using System;

    internal class CombinedTestResult
    {
        public CombinedTestResult(string cachedResult, string result)
        {
            this.CachedResult = cachedResult;
            this.Result = result;
        }

        public CombinedTestResult(string commonResult)
            : this(commonResult, commonResult)
        {
        }

        public string CachedResult { get; }

        public string Result { get; }

        public override bool Equals(object obj)
        {
            var testResult = obj as CombinedTestResult;
            if (obj == null || testResult == null)
            {
                return false;
            }

            return this.Equals(testResult);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.CachedResult?.GetHashCode() ?? 0) * 397) ^ (this.Result?.GetHashCode() ?? 0);
            }
        }

        public override string ToString()
        {
            return $"CachedResult: {this.CachedResult}, Result: {this.Result}";
        }

        protected bool Equals(CombinedTestResult other) => string.Equals(this.CachedResult, other.CachedResult) && string.Equals(this.Result, other.Result);
    }
}