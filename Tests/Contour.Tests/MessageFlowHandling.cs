using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contour.Caching;
using Contour.Flow.Configuration;
using Contour.Flow.Execution;
using Contour.Flow.Transport;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
// ReSharper disable InconsistentNaming

namespace Contour.Tests
{
    public class MessageFlowHandling
    {
        [TestFixture]
        public class describe_flow
        {
            [Test]
            public void should_buffer_incoming_messages()
            {
                var factory = GetMessageFlowFactory();
                var flow = factory.Create<Payload>("fake");
                flow.On("incoming_label");

                flow.Entry().Post(new Payload());
            }

            [Test]
            public void should_perform_single_action_on_incoming_message()
            {
                var factory = GetMessageFlowFactory();
                var flow = factory.Create<Payload>("fake_transport");
                var action = A.Fake<Func<FlowContext<Payload>, Payload>>();

                flow.On("incoming_label")
                    .Act(action);

                var tcs = new TaskCompletionSource<bool>();
                A.CallTo(action).Invokes(() => tcs.SetResult(true));

                flow.Entry().Post(new Payload());

                tcs.Task.Wait(1.Minutes());
                A.CallTo(action).MustHaveHappened();
            }

            [Test]
            public void should_perform_single_action_with_no_result_on_incoming_message()
            {
                var factory = GetMessageFlowFactory();
                var flow = factory.Create<Payload>("fake");
                var action = A.Fake<Action<FlowContext<Payload>>>();

                flow.On("incoming_label")
                    .Act(action);

                var tcs = new TaskCompletionSource<bool>();
                A.CallTo(action).Invokes(() => tcs.SetResult(true));
                flow.Entry().Post(new Payload());

                tcs.Task.Wait(1.Minutes());
                A.CallTo(action).MustHaveHappened();
            }

            [Test]
            public void should_terminate_the_flow_on_action_errors()
            {
                //var factory = GetMessageFlowFactory();
                //var flow = factory.Create<Payload>("fake");
                //var tcs = new TaskCompletionSource<bool>();

                //var errorAction = A.Fake<Func<Payload, NewPayload>>();
                //A.CallTo(errorAction).WithAnyArguments().Throws(new Exception());
                //A.CallTo(errorAction).Invokes(() => tcs.SetResult(true));

                //var nextAction = A.Fake<Func<FlowContext<Payload, NewPayload>, NewPayload>>();
                //A.CallTo(nextAction).DoesNothing();

                //flow.On("incoming_label")
                //    .Act(errorAction)
                //    .Act(nextAction);

                //flow.Entry.Post(new Payload());
                //tcs.Task.Wait(AMinute());

                //A.CallTo(errorAction).MustHaveHappened();
                //A.CallTo(nextAction).MustNotHaveHappened();
            }

            [Test]
            public void should_cache_action_results()
            {
                var putTcs = new TaskCompletionSource<bool>();
                var getTcs = new TaskCompletionSource<bool>();

                var @in = new Payload() {Data = "123"};
                var @out = new NewPayload() {Value = 123};
                var period = TimeSpan.FromMinutes(5);

                var hashKeyProvider = new HashKeyProvider();
                var key = hashKeyProvider.Get(@in);

                var cacheProvider = A.Fake<ICacheProvider>();

                NewPayload cached = null;
                //fake a cache miss and a subsequent cache hit
                A.CallTo(() => cacheProvider.Get<NewPayload>(key)).Returns(null);
                    //or throws exception in some cache implementations
                A.CallTo(() => cacheProvider.Put(key, @out, period)).Invokes(() =>
                {
                    cached = @out;
                    putTcs.SetResult(true);
                });

                var policy = A.Fake<ICachePolicy>();
                A.CallTo(() => policy.CacheProvider).Returns(cacheProvider);
                A.CallTo(() => policy.KeyProvider).Returns(hashKeyProvider);
                A.CallTo(() => policy.Period).Returns(period);

                var factory = GetMessageFlowFactory();
                var flow = factory.Create<Payload>("fake");
                flow.On("incoming_label")
                    .Act(payload => @out, policy: policy);

                flow.Entry().Post(@in);
                putTcs.Task.Wait(1.Minutes());
                cached.Should().Be(@out);

                //check cache hit
                NewPayload fetched = null;
                A.CallTo(() => cacheProvider.Get<NewPayload>(key))
                    .Invokes(() =>
                    {
                        fetched = @out;
                        getTcs.SetResult(true);
                    })
                    .Returns(@out);

                flow.Entry().Post(@in);
                getTcs.Task.Wait(1.Minutes());
                fetched.Should().Be(@out);
            }
        }

        private static IFlowFactory GetMessageFlowFactory()
        {
            var factory = A.Fake<IFlowFactory>();
            A.CallTo(() => factory.Create<Payload>(A.Dummy<string>()))
                .WithAnyArguments()
                .ReturnsLazily(() => new LocalMessageFlow<Payload>(new LocalFlowTransport()));
            return factory;
        }
    }
}