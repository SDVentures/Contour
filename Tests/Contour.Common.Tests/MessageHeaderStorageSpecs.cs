using System.Collections.Generic;

using NUnit.Framework;

namespace Contour.Common.Tests
{
    /// <summary>
    /// Тесты для проверки хранилища заголовков входящего сообщения.
    /// </summary>
    public class MessageHeaderStorageSpecs
    {
        /// <summary>
        /// При сохранении заголовков.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class WhenStoreHeaders
        {
            /// <summary>
            /// Должны исключаться заголовки из черного списка.
            /// </summary>
            [Test]
            public void ShouldFilterHeadersOfBlackList()
            {
                var blackList = new List<string> { "b", "c" };

                var sut = new MessageHeaderStorage(blackList);

                sut.Store(new Dictionary<string, object> { { "a", 1 }, { "b", 2 }, { "c", 3 }, { "d", 4 } });

                CollectionAssert.DoesNotContain(sut.Load().Keys, "b", "Заголовок должен быть исключен при сохранении.");
                CollectionAssert.DoesNotContain(sut.Load().Keys, "c", "Заголовок должен быть исключен при сохранении.");
            }
        }
    }
}
