using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NSpec;
using NSpec.Domain;
using NSpec.Domain.Formatters;
using NUnit.Framework;

namespace Contour.Tests
{
    [TestFixture]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    // ReSharper disable once InconsistentNaming
    public abstract class NSpec : global::NSpec.nspec
    {
        [Test]
        // ReSharper disable once InconsistentNaming
        public void debug()
        {
            var currentSpec = this.GetType();
            var finder = new SpecFinder(new[] { currentSpec });
            var tagsFilter = new Tags().Parse(currentSpec.Name);

            var builder = new ContextBuilder(finder, tagsFilter, new DefaultConventions());
            var runner = new ContextRunner(tagsFilter, new ConsoleFormatter(), false);
            var results = runner.Run(builder.Contexts().Build());

            results.Failures().Count().should_be(0);
        }
    }
}
