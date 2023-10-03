using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace PBase.Logging
{
    public static class PLog
    {
        private static ILoggerFactory sm_factory = null;
        //private static ILogger sm_logger = null;
        private static readonly AsyncLocal<ILogger> sm_logger = new AsyncLocal<ILogger>();

        public static ILoggerFactory LoggerFactory => sm_factory;
        public static ILogger Logger => sm_logger.Value;

        public static void AssignLoggerFactory(ILoggerFactory factory)
        {
            sm_factory = factory;

            sm_logger.Value = CreateLogger("PBase");
        }

        public static ILogger CreateLogger(string category) => LoggerFactory.CreateLogger(category);

        public static ILogger<T> CreateLogger<T>() => LoggerFactory.CreateLogger<T>();

        public static void LogCritical(string message, params object[] args)
        {
            sm_logger.Value?.LogCritical(message, args);
        }
        public static void LogCritical(Exception exception, string message, params object[] args)
        {
            sm_logger.Value?.LogCritical(exception, message, args);

        }
        public static void LogCritical(EventId eventId, string message, params object[] args)
        {
            sm_logger.Value?.LogCritical(eventId, message, args);
        }
        public static void LogCritical(EventId eventId, Exception exception, string message, params object[] args)
        {
            sm_logger.Value?.LogCritical(eventId, exception, message, args);
        }
        public static void LogDebug(EventId eventId, Exception exception, string message, params object[] args)
        {
            sm_logger.Value?.LogDebug(eventId, exception, message, args);
        }
        public static void LogDebug(EventId eventId, string message, params object[] args)
        {
            sm_logger.Value?.LogDebug(eventId, message, args);
        }
        public static void LogDebug(Exception exception, string message, params object[] args)
        {
            sm_logger.Value?.LogDebug(exception, message, args);
        }
        public static void LogDebug(string message, params object[] args)
        {
            sm_logger.Value?.LogDebug(message, args);
        }
        public static void LogError(string message, params object[] args)
        {
            sm_logger.Value?.LogError(message, args);
        }
        public static void LogError(Exception exception, string message, params object[] args)
        {
            sm_logger.Value?.LogError(exception, message, args);
        }
        public static void LogError(EventId eventId, Exception exception, string message, params object[] args)
        {
            sm_logger.Value?.LogError(eventId, exception, message, args);
        }
        public static void LogError(EventId eventId, string message, params object[] args)
        {
            sm_logger.Value?.LogError(eventId, message, args);
        }
        public static void LogInformation(EventId eventId, string message, params object[] args)
        {
            sm_logger.Value?.LogInformation(eventId, message, args);
        }
        public static void LogInformation(Exception exception, string message, params object[] args)
        {
            sm_logger.Value?.LogInformation(exception, message, args);
        }
        public static void LogInformation(EventId eventId, Exception exception, string message, params object[] args)
        {
            sm_logger.Value?.LogInformation(eventId, exception, message, args);
        }
        public static void LogInformation(string message, params object[] args)
        {
            sm_logger.Value?.LogInformation(message, args);
        }
        public static void LogTrace(string message, params object[] args)
        {
            sm_logger.Value?.LogTrace(message, args);
        }
        public static void LogTrace(Exception exception, string message, params object[] args)
        {
            sm_logger.Value?.LogTrace(exception, message, args);
        }
        public static void LogTrace(EventId eventId, string message, params object[] args)
        {
            sm_logger.Value?.LogTrace(eventId, message, args);
        }
        public static void LogTrace(EventId eventId, Exception exception, string message, params object[] args)
        {
            sm_logger.Value?.LogTrace(eventId, exception, message, args);
        }
        public static void LogWarning(EventId eventId, string message, params object[] args)
        {
            sm_logger.Value?.LogWarning(eventId, message, args);
        }
        public static void LogWarning(EventId eventId, Exception exception, string message, params object[] args)
        {
            sm_logger.Value?.LogWarning(eventId, exception, message, args);
        }
        public static void LogWarning(string message, params object[] args)
        {
            sm_logger.Value?.LogWarning(message, args);
        }
        public static void LogWarning(Exception exception, string message, params object[] args)
        {
            sm_logger.Value?.LogWarning(exception, message, args);
        }
    }
}
