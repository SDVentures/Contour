using Contour.Transport.RabbitMQ.Topology;

using NUnit.Framework;

namespace Contour.Common.Tests
{
    /// <summary>
    /// Набор тестов для генератора имен.
    /// </summary>
    public class NameGeneratorSpecs
    {
        /// <summary>
        /// При генерации случайного имени.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class WhenGenerateRandomName
        {
            /// <summary>
            /// Должен вернуть один символ.
            /// </summary>
            [Test]
            public void ShouldReturnOneSymbol()
            {
                var result = NameGenerator.GetRandomName(1);

                Assert.AreEqual(1, result.Length);
            }

            /// <summary>
            /// Должен вернуть пустую строку.
            /// </summary>
            [Test]
            public void ShouldReturnZeroSymbols()
            {
                var result = NameGenerator.GetRandomName(0);

                Assert.AreEqual(0, result.Length);
            }

            /// <summary>
            /// Должен 5 символов.
            /// </summary>
            [Test]
            public void ShouldReturnFiveSymbols()
            {
                var result = NameGenerator.GetRandomName(5);

                Assert.AreEqual(5, result.Length);
            }

            /// <summary>
            /// Должен 40 символов.
            /// </summary>
            [Test]
            public void ShouldReturnFortySymbols()
            {
                var result = NameGenerator.GetRandomName(40);

                Assert.AreEqual(40, result.Length);
            }
        }
    }
}
