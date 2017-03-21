using System;
using System.Diagnostics.CodeAnalysis;
using Contour.Transport.RabbitMQ;
using NUnit.Framework;

namespace Contour.RabbitMq.Tests
{
    /// <summary>
    /// The rabbit connection string specs.
    /// </summary>
    public class RabbitConnectionStringSpecs
    {
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here."),SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here."),TestFixture]
        [Category("Unit")]
        public class when_declaring_rabbit_connection_string
        {
            [Test]
            public void should_use_not_null_or_empty_string()
            {
                string connectionString = null;

                try
                {
                    new RabbitConnectionString(connectionString);
                    Assert.Fail();
                }
                catch (Exception)
                {
                }
            }
            
            [Test]
            public void should_enumerate_connection_string_urls_using_enumeration_algorithm()
            {
            }
        }
    }
}