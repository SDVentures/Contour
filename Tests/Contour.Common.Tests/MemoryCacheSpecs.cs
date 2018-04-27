using System.Diagnostics.CodeAnalysis;

namespace Contour.Common.Tests
{
    using System;
    using System.Threading;

    using FluentAssertions;

    using Contour.Caching;
    using Contour.Helpers;

    using NUnit.Framework;

    /// <summary>
    /// The memory cache specs.
    /// </summary>
    // ReSharper disable InconsistentNaming
    public class MemoryCacheSpecs
    {
        /// <summary>
        /// The boo.
        /// </summary>
        public class Boo
        {
            #region Public Properties

            /// <summary>
            /// Gets or sets the num.
            /// </summary>
            public int Num { get; set; }

            /// <summary>
            /// Gets or sets the str.
            /// </summary>
            public string Str { get; set; }

            #endregion
        }

        /// <summary>
        /// The when_finding_non_existing_value_from_cache.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_finding_non_existing_value_from_cache
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_not_throw.
            /// </summary>
            [Test]
            public void should_not_throw()
            {
                var cacheProvider = new MemoryCacheProvider();
                Maybe<Boo> value = null;

                cacheProvider.Invoking(c => { value = c.Find<Boo>("key"); }).
                    ShouldNotThrow();
                value.HasValue.Should().
                    BeFalse();
            }

            #endregion
        }

        /// <summary>
        /// The when_getting_expired_value_from_cache.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_getting_expired_value_from_cache
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_fail.
            /// </summary>
            [Test]
            public void should_fail()
            {
                var cacheProvider = new MemoryCacheProvider();

                cacheProvider.Put("key", new Boo { Num = 13 }, 100.Milliseconds());

                Thread.Sleep(500);

                cacheProvider.Find<Boo>("key").
                    HasValue.Should().
                    BeFalse();
            }

            #endregion
        }

        /// <summary>
        /// The when_getting_expired_value_from_cache_using_absolute_expiration.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_getting_expired_value_from_cache_using_absolute_expiration
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_fail.
            /// </summary>
            [Test]
            public void should_fail()
            {
                var cacheProvider = new MemoryCacheProvider();

                cacheProvider.Put("key", new Boo { Num = 13 }, DateTime.Now.AddMilliseconds(100));

                Thread.Sleep(500);

                cacheProvider.Find<Boo>("key").
                    HasValue.Should().
                    BeFalse();
            }

            #endregion
        }

        /// <summary>
        /// The when_getting_non_existing_value_from_cache.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_getting_non_existing_value_from_cache
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_throw.
            /// </summary>
            [Test]
            public void should_throw()
            {
                var cacheProvider = new MemoryCacheProvider();

                cacheProvider.Invoking(c => c.Get<Boo>("key")).
                    ShouldThrow<InvalidOperationException>();
            }

            #endregion
        }

        /// <summary>
        /// The when_getting_removed_value_from_cache.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_getting_removed_value_from_cache
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_fail.
            /// </summary>
            [Test]
            public void should_fail()
            {
                var cacheProvider = new MemoryCacheProvider();

                cacheProvider.Put("key", new Boo { Num = 13 }, 5.Seconds());
                cacheProvider.Remove("key");

                cacheProvider.Find<Boo>("key").
                    HasValue.Should().
                    BeFalse();
            }

            #endregion
        }

        /// <summary>
        /// The when_getting_value_from_cache_before_expiration_date.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_getting_value_from_cache_before_expiration_date
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_return_value.
            /// </summary>
            [Test]
            public void should_return_value()
            {
                var cacheProvider = new MemoryCacheProvider();

                cacheProvider.Put("key", new Boo { Num = 13 }, DateTime.Now.AddSeconds(5));
                var value = cacheProvider.Get<Boo>("key");

                value.Num.Should().
                    Be(13);
            }

            #endregion
        }

        /// <summary>
        /// The when_getting_value_from_cache_within_expiration_period.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_getting_value_from_cache_within_expiration_period
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_return_value.
            /// </summary>
            [Test]
            public void should_return_value()
            {
                var cacheProvider = new MemoryCacheProvider();

                cacheProvider.Put("key", new Boo { Num = 13 }, 5.Seconds());
                var value = cacheProvider.Get<Boo>("key");

                value.Num.Should().
                    Be(13);
            }

            #endregion
        }

        /// <summary>
        /// Проверяет, что при добавлении элемента в кеш, на основе интервала, создается элемент кеша с абсолютным временем.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        [Category("Unit")]
        public class when_put_item_with_timespan
        {
            /// <summary>
            /// Должен быть создан элемент кеша с абсолютным временем.
            /// </summary>
            [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
            [Test]
            public void should_absolutly_expired()
            {
                var cacheProvider = new MemoryCacheProvider();

                cacheProvider.Put("key", new Boo { Num = 13 }, 5.Seconds());

                Thread.Sleep(3000);
                cacheProvider.Get<Boo>("key");
                Thread.Sleep(3000);

                Assert.Throws<InvalidOperationException>(() => cacheProvider.Get<Boo>("key"), "Объект должен быть удален из кеша.");
            }
        }
    }

    // ReSharper restore InconsistentNaming
}
