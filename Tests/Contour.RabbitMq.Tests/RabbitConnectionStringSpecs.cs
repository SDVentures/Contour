using System;
using System.Diagnostics.CodeAnalysis;
using Contour.Transport.RabbitMq;
using FluentAssertions;
using NUnit.Framework;
// ReSharper disable InconsistentNaming

namespace Contour.RabbitMq.Tests
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
        Justification = "Reviewed. Suppression is OK here."),
     SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter",
         Justification = "Reviewed. Suppression is OK here.")]
    public class RabbitConnectionStringSpecs
    {
        [TestFixture]
        [Category("Unit")]
        public class when_declaring_rabbit_connection_string
        {
            [Test]
            public void should_use_not_null_or_empty_string()
            {
                Assert.Throws<ArgumentNullException>(() => new RabbitConnectionString(null));
                Assert.Throws<ArgumentNullException>(() => new RabbitConnectionString(string.Empty));
            }

            [Test]
            public void should_use_string_with_at_least_one_entry()
            {
                Assert.Throws<ArgumentException>(() => new RabbitConnectionString(" "));
            }

            [Test]
            public void should_use_not_emtry_urls()
            {
                Assert.Throws<ArgumentException>(() => new RabbitConnectionString(" , , "));
            }

            [Test]
            public void should_enumerate_connection_string_urls()
            {
                var strings = new[] { "a", "b", "c" };
                var connectionString = string.Join(",", strings);

                var result = new RabbitConnectionString(connectionString);
                result.Should().HaveCount(strings.Length);

                var i = 0;
                foreach (var url in result)
                {
                    url.Should().Be(strings[i++]);
                }
            }
        }
    }
}