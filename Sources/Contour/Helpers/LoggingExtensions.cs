using System;
using Common.Logging;

namespace Contour.Helpers
{
    internal static class LoggingExtensions
    {
        public static void Trace(this ILog logger, Func<string> log)
        {
            logger.Trace(m => m("{0}", log()));
        }

        public static void Debug(this ILog logger, Func<string> log)
        {
            logger.Debug(m => m("{0}", log()));
        }

        public static void Warn(this ILog logger, Func<string> log, Exception exception = null)
        {
            if (exception == null)
            {
                logger.Warn(m => m("{0}", log()));
            }
            else
            {
                logger.Warn(m => m("{0}", log()), exception);
            }
        }

        public static void Error(this ILog logger, Func<string> log)
        {
            logger.Error(m => m("{0}", log()));
        }

        public static void Error(this ILog logger, Func<string> log, Exception exception)
        {
            logger.Error(m => m("{0}", log()), exception);
        }
    }
}
