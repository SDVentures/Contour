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
                var excludedList = new List<string> { "b", "c" };

                var excludedListAddition = new List<string> { "e", "f" };

                var sut = new MessageHeaderStorage(excludedList);

                sut.RegisterExcludedHeaders(excludedListAddition);

                sut.Store(new Dictionary<string, object> { { "a", 1 }, { "b", 2 }, { "c", 3 }, { "d", 4 }, { "e", 5 }, { "f", 6 }, { "g", 7 } });

                var storedResult = sut.Load();

                CollectionAssert.DoesNotContain(storedResult.Keys, "b", "Заголовок должен быть исключен при сохранении.");
                CollectionAssert.DoesNotContain(storedResult.Keys, "c", "Заголовок должен быть исключен при сохранении.");
                CollectionAssert.DoesNotContain(storedResult.Keys, "e", "Заголовок должен быть исключен при сохранении.");
                CollectionAssert.DoesNotContain(storedResult.Keys, "f", "Заголовок должен быть исключен при сохранении.");
            }
        }
    }
}
