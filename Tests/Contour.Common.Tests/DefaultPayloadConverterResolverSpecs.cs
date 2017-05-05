using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Contour.Serialization;
using Contour.Transport.RabbitMq.Internal;

using Moq;

using NUnit.Framework;
// ReSharper disable InconsistentNaming

namespace Contour.Common.Tests
{
    [TestFixture]
    [Category("Unit")]
    public class DefaultPayloadConverterResolverSpecs
    {
        [Test]
        public void when_no_such_converters_should_throw_exception()
        {
            var sut = new DefaultPayloadConverterResolver(
                new ReadOnlyCollection<IPayloadConverter>(new List<IPayloadConverter>
                                                              {
                                                                new StubPayloadConverter("application/protobuf"),
                                                                new StubPayloadConverter("application/json"),
                                                              }));

            Assert.Throws<ArgumentOutOfRangeException>(() => sut.ResolveConverter("application/json22222"));
        }

        [Test]
        public void when_converter_with_specific_content_type_presents_should_return_it()
        {
            var jsonConverter = new StubPayloadConverter("application/json");
            var sut = new DefaultPayloadConverterResolver(
                new ReadOnlyCollection<IPayloadConverter>(
                    new List<IPayloadConverter>
                        {
                            new StubPayloadConverter("application/protobuf"),
                            jsonConverter
                        }));

            var converter = sut.ResolveConverter("application/json");

            Assert.AreEqual(converter, jsonConverter);
        }

        [Test]
        public void when_no_message_content_type_should_return_first_converter()
        {
            var jsonConverter = new StubPayloadConverter("application/json");
            var sut = new DefaultPayloadConverterResolver(
                new ReadOnlyCollection<IPayloadConverter>(
                    new List<IPayloadConverter>
                        {
                            jsonConverter,
                            new StubPayloadConverter("application/protobuf")
                        }));

            var converter = sut.ResolveConverter("application/json");

            Assert.AreEqual(converter, jsonConverter);
        }

        private class StubPayloadConverter : IPayloadConverter
        {
            public StubPayloadConverter(string contentType)
            {
                this.ContentType = contentType;
            }

            public string ContentType { get; }

            public byte[] FromObject(object payload)
            {
                return new byte[0];
            }

            public object ToObject(byte[] payload, Type targetType)
            {
                return new object();
            }
        }
    }
}