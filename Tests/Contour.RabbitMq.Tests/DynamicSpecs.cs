using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Text;
using System.Threading;

using FluentAssertions;

using Contour.Sending;
using Contour.Serialization;
using Contour.Testing.Transport.RabbitMq;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using NUnit.Framework;
namespace Contour.RabbitMq.Tests
{
    // ReSharper disable InconsistentNaming
    /// <summary>
    /// The dynamic specs.
    /// </summary>
    public class DynamicSpecs
    {
        /// <summary>
        /// The when_consuming_non_cls_compliant_message.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_consuming_non_cls_compliant_message : RabbitMqFixture
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_deserialize_message_to_valid_clr_object.
            /// </summary>
            [Test]
            [Explicit("A broken experiment.")]
            public void should_deserialize_message_to_valid_clr_object()
            {
                int result = 0;
                var waitHandle = new AutoResetEvent(false);

                IBus producer = this.StartBus("producer", cfg => cfg.Route("boo"));

                this.StartBus(
                    "consumer",
                    cfg =>
                        {
                            cfg.UsePayloadConverter(new SmartJsonNetSerializer());
                            cfg.On<ExpandoObject>("boo").
                                ReactWith(
                                    (m, ctx) =>
                                        {
                                            result = ((dynamic)m).NumValue;
                                            waitHandle.Set();
                                        });
                        });

                producer.Emit("boo", new Dictionary<string, object> { { "num-value", 13 } });

                waitHandle.WaitOne(5.Seconds()).
                    Should().
                    BeTrue();

                result.Should().
                    Be(13);
            }

            #endregion

            /// <summary>
            /// The lower case delimited property names contract resovler.
            /// </summary>
            public class LowerCaseDelimitedPropertyNamesContractResovler : DefaultContractResolver
            {
                #region Fields

                /// <summary>
                /// The _delimiter.
                /// </summary>
                private readonly char _delimiter;

                #endregion

                #region Constructors and Destructors

                /// <summary>
                /// »нициализирует новый экземпл€р класса <see cref="LowerCaseDelimitedPropertyNamesContractResovler"/>.
                /// </summary>
                /// <param name="delimiter">
                /// The delimiter.
                /// </param>
                public LowerCaseDelimitedPropertyNamesContractResovler(char delimiter)
                    : base(true)
                {
                    this._delimiter = delimiter;
                }

                #endregion

                #region Methods

                /// <summary>
                /// The create property.
                /// </summary>
                /// <param name="member">
                /// The member.
                /// </param>
                /// <param name="memberSerialization">
                /// The member serialization.
                /// </param>
                /// <returns>
                /// The <see cref="JsonProperty"/>.
                /// </returns>
                protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
                {
                    JsonProperty res = base.CreateProperty(member, memberSerialization);
                    res.PropertyName = this.FromSnakeToPascal(res.PropertyName);
                    return res;
                }

                /// <summary>
                /// The resolve property name.
                /// </summary>
                /// <param name="propertyName">
                /// The property name.
                /// </param>
                /// <returns>
                /// The <see cref="string"/>.
                /// </returns>
                protected override string ResolvePropertyName(string propertyName)
                {
                    return propertyName; // .ToDelimitedString(_delimiter);
                }

                /// <summary>
                /// The from snake to pascal.
                /// </summary>
                /// <param name="str">
                /// The str.
                /// </param>
                /// <returns>
                /// The <see cref="string"/>.
                /// </returns>
                private string FromSnakeToPascal(string str)
                {
                    string[] parts = str.Split(new[] { this._delimiter });
                    var sb = new StringBuilder();

                    foreach (string part in parts)
                    {
                        sb.Append(parts[0].ToUpper() + part.Substring(1));
                    }

                    return sb.ToString();
                }

                #endregion
            }

            /// <summary>
            /// The smart json net serializer.
            /// </summary>
            private class SmartJsonNetSerializer : IPayloadConverter
            {
                #region Public Properties

                /// <summary>
                /// Gets the content type.
                /// </summary>
                public string ContentType
                {
                    get
                    {
                        return "application/json";
                    }
                }

                #endregion

                #region Public Methods and Operators

                /// <summary>
                /// The from object.
                /// </summary>
                /// <param name="payload">
                /// The payload.
                /// </param>
                /// <returns>
                /// The <see cref="byte[]"/>.
                /// </returns>
                public byte[] FromObject(object payload)
                {
                    string json = JsonConvert.SerializeObject(payload);

                    return Encoding.UTF8.GetBytes(json);
                }

                /// <summary>
                /// The to object.
                /// </summary>
                /// <param name="payload">
                /// The payload.
                /// </param>
                /// <param name="targetType">
                /// The target type.
                /// </param>
                /// <returns>
                /// The <see cref="object"/>.
                /// </returns>
                public object ToObject(byte[] payload, Type targetType)
                {
                    string decoded = Encoding.UTF8.GetString(payload);
                    var serializerSettings = new JsonSerializerSettings { ContractResolver = new LowerCaseDelimitedPropertyNamesContractResovler('-') };

                    return JsonConvert.DeserializeObject(decoded, targetType, serializerSettings);
                }

                #endregion
            }
        }

        /// <summary>
        /// The when_publishing_non_cls_compliant_message.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_publishing_non_cls_compliant_message : RabbitMqFixture
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_catch_message_on_subscribed_consumer.
            /// </summary>
            [Test]
            public void should_catch_message_on_subscribed_consumer()
            {
                int result = 0;
                var waitHandle = new AutoResetEvent(false);

                IBus producer = this.StartBus("producer", cfg => cfg.Route("boo"));

                this.StartBus(
                    "consumer",
                    cfg => cfg.On<ExpandoObject>("boo").
                               ReactWith(
                                   (m, ctx) =>
                                       {
                                           result = (int)(long)((IDictionary<string, object>)m)["num-value"];
                                           waitHandle.Set();
                                       }));

                producer.Emit("boo", new Dictionary<string, object> { { "num-value", 13 } });

                waitHandle.WaitOne(5.Seconds()).
                    Should().
                    BeTrue();

                result.Should().
                    Be(13);
            }

            #endregion
        }

        /// <summary>
        /// The when_receiving_using_dynamic.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_receiving_using_dynamic : RabbitMqFixture
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_deserialize_all_data.
            /// </summary>
            [Test]
            public void should_deserialize_all_data()
            {
                int result = 0;
                var waitHandle = new AutoResetEvent(false);

                IBus producer = this.StartBus("producer", cfg => cfg.Route("boo.message"));

                this.StartBus(
                    "consumer",
                    cfg => cfg.On<dynamic>("boo.message").
                               ReactWith(
                                   (m, ctx) =>
                                       {
                                           result = m.Num;
                                           waitHandle.Set();
                                       }));

                producer.Emit("boo.message", new BooMessage(13));

                waitHandle.WaitOne(5.Seconds()).
                    Should().
                    BeTrue();

                result.Should().
                    Be(13);
            }

            #endregion
        }

        /// <summary>
        /// The when_removing_data_from_expandoobject_messages.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_removing_data_from_expandoobject_messages : RabbitMqFixture
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_apply_transformation.
            /// </summary>
            [Test]
            public void should_apply_transformation()
            {
                dynamic result = null;

                IBus producer = this.StartBus(
                    "producer",
                    cfg => cfg.Route("boo.message").
                               WithDefaultCallbackEndpoint());

                this.StartBus(
                    "consumer",
                    cfg => cfg.On<ExpandoObject>("boo.message").
                               ReactWith(
                                   (m, ctx) =>
                                       {
                                           var dict = (IDictionary<string, object>)m;

                                           dict["Num"] = ((string)dict["Str"]).Length;
                                           dict.Remove("Obsolete");

                                           ctx.Reply(m);
                                           ctx.Accept();
                                       }));

                dynamic message = new ExpandoObject();
                message.Str = new String('*', 13);
                message.Obsolete = "not-needed";

                producer.Request<dynamic, dynamic>("boo.message", (object)message, new RequestOptions { Timeout = TimeSpan.FromSeconds(5) }, o => { result = o; });

                ((int)result.Num).Should().
                    Be(13);
                ((string)result.Str).Should().
                    NotBeNull();
                ((string)result.Obsolete).Should().
                    BeNull();
            }

            #endregion
        }

        /// <summary>
        /// The when_sending_using_dynamic.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_sending_using_dynamic : RabbitMqFixture
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_serialize_all_data.
            /// </summary>
            [Test]
            public void should_serialize_all_data()
            {
                int result = 0;
                var waitHandle = new AutoResetEvent(false);

                IBus producer = this.StartBus("producer", cfg => cfg.Route("boo.message"));

                this.StartBus(
                    "consumer",
                    cfg => cfg.On<BooMessage>("boo.message").
                               ReactWith(
                                   (m, ctx) =>
                                       {
                                           result = m.Num;
                                           waitHandle.Set();
                                       }));

                dynamic message = new ExpandoObject();
                message.Num = 13;
                producer.Emit("boo.message", message);

                waitHandle.WaitOne(5.Seconds()).
                    Should().
                    BeTrue();

                result.Should().
                    Be(13);
            }

            #endregion
        }

        /// <summary>
        /// The when_transforming_dynamic_messages.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_transforming_dynamic_messages : RabbitMqFixture
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_apply_transformation.
            /// </summary>
            [Test]
            public void should_apply_transformation()
            {
                dynamic result = null;

                IBus producer = this.StartBus(
                    "producer",
                    cfg => cfg.Route("boo.message").
                               WithDefaultCallbackEndpoint());

                this.StartBus(
                    "consumer",
                    cfg => cfg.On<dynamic>("boo.message").
                               ReactWith(
                                   (m, ctx) =>
                                       {
                                           m.Num = ((string)m.Str).Length;
                                           m.Obsolete = null;

                                           ctx.Reply(m);
                                           ctx.Accept();
                                       }));

                dynamic message = new ExpandoObject();
                message.Str = new String('*', 13);
                message.Obsolete = "not-needed";

                producer.Request<dynamic, dynamic>("boo.message", (object)message, new RequestOptions { Timeout = TimeSpan.FromSeconds(5) }, o => { result = o; });

                ((int)result.Num).Should().
                    Be(13);
                ((string)result.Str).Should().
                    NotBeNull();
                ((string)result.Obsolete).Should().
                    BeNull();
            }

            #endregion
        }
    }

    // ReSharper restore InconsistentNaming
}
