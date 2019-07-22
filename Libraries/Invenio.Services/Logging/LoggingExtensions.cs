using System;
using Invenio.Core.Domain.Users;
using Invenio.Core.Domain.Logging;

namespace Invenio.Services.Logging
{
    public static class LoggingExtensions
    {
        public static void Debug(this ILogger logger, string message, Exception exception = null, User User = null)
        {
            FilteredLog(logger, LogLevel.Debug, message, exception, User);
        }
        public static void Information(this ILogger logger, string message, Exception exception = null, User User = null)
        {
            FilteredLog(logger, LogLevel.Information, message, exception, User);
        }
        public static void Warning(this ILogger logger, string message, Exception exception = null, User User = null)
        {
            FilteredLog(logger, LogLevel.Warning, message, exception, User);
        }
        public static void Error(this ILogger logger, string message, Exception exception = null, User User = null)
        {
            FilteredLog(logger, LogLevel.Error, message, exception, User);
        }
        public static void Fatal(this ILogger logger, string message, Exception exception = null, User User = null)
        {
            FilteredLog(logger, LogLevel.Fatal, message, exception, User);
        }

        private static void FilteredLog(ILogger logger, LogLevel level, string message, Exception exception = null, User User = null)
        {
            //don't log thread abort exception
            if (exception is System.Threading.ThreadAbortException)
                return;

            if (logger.IsEnabled(level))
            {
                string fullMessage = exception == null ? string.Empty : exception.ToString();
                logger.InsertLog(level, message, fullMessage, User);
            }
        }
    }
}
