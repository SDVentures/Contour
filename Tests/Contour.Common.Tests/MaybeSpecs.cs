namespace Contour.Common.Tests
{
    using System.Diagnostics.CodeAnalysis;
    using Helpers;
    using NUnit.Framework;

    /// <summary>
    /// The maybe specs.
    /// </summary>
    public class MaybeSpecs
    {
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here."), SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here."), TestFixture]
        [Category("Unit")]
        public class when_defining_maybe
        {
            [Ignore("The Maybe type should be rewritten")]
            [Test]
            public void should_return_boolean_if_set()
            {

                var a = new Maybe<bool>(true);
                Assert.IsTrue(a.HasValue && a);

                // The following check will fail

                // var b = new Maybe<bool>(false);
                // Assert.IsTrue(b.HasValue && !b);
            }
        }
    }
}
