namespace Contour.Common.Tests
{
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using FluentAssertions;

    using NUnit.Framework;

    /// <summary>
    /// The task completion source specs.
    /// </summary>
    // ReSharper disable InconsistentNaming
    public class TaskCompletionSourceSpecs
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get threads count.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int GetThreadsCount()
        {
            return Process.GetCurrentProcess().
                Threads.OfType<ProcessThread>().
                Count( /*t => t.ThreadState == System.Diagnostics.ThreadState.Running*/);
        }

        #endregion

        /// <summary>
        /// The when_waiting_on_incomplete_task_completion_source.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_waiting_on_incomplete_task_completion_source
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_use_threads_while_waiting.
            /// </summary>
            [Test]
            [Ignore("Unstable, just for fun")]
            public void should_use_threads_while_waiting()
            {
                int initialThreadCount = GetThreadsCount();

                var tcs1 = new TaskCompletionSource<int>();
                var tcs2 = new TaskCompletionSource<int>();
                var tcs3 = new TaskCompletionSource<int>();

                int tcsCreatedThreadCount = GetThreadsCount();

                var cancellation = new CancellationTokenSource();
                Task.Factory.StartNew(() => Task.WaitAny(new[] { tcs1.Task, tcs2.Task, tcs3.Task }, cancellation.Token));

                int waitingThreadCount = GetThreadsCount();

                try
                {
                    tcsCreatedThreadCount.Should().
                        Be(initialThreadCount);
                    waitingThreadCount.Should().
                        Be(initialThreadCount + 3);
                }
                finally
                {
                    cancellation.Cancel();
                }
            }

            #endregion
        }
    }

    // ReSharper restore InconsistentNaming
}
