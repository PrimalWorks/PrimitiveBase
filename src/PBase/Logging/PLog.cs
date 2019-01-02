using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace PBase.Logging
{
    public static class PLog
    {
        private static ILoggerFactory sm_factory = null;
        private static ILogger sm_logger = null;

        public static ILoggerFactory LoggerFactory => sm_factory;
        public static ILogger Logger => sm_logger;

        public static void AssignLoggerFactory(ILoggerFactory factory)
        {
            sm_factory = factory;

            sm_logger = CreateLogger("PBase");
        }

        public static ILogger CreateLogger(string category) => LoggerFactory.CreateLogger(category);

        public static ILogger<T> CreateLogger<T>() => LoggerFactory.CreateLogger<T>();

        public static void LogCritical(string message, params object[] args)
        {
            sm_logger?.LogCritical(message, args);
        }
        public static void LogCritical(Exception exception, string message, params object[] args)
        {
            sm_logger?.LogCritical(exception, message, args);

        }
        public static void LogCritical(EventId eventId, string message, params object[] args)
        {
            sm_logger?.LogCritical(eventId, message, args);
        }
        public static void LogCritical(EventId eventId, Exception exception, string message, params object[] args)
        {
            sm_logger?.LogCritical(eventId, exception, message, args);
        }
        public static void LogDebug(EventId eventId, Exception exception, string message, params object[] args)
        {
            sm_logger?.LogDebug(eventId, exception, message, args);
        }
        public static void LogDebug(EventId eventId, string message, params object[] args)
        {
            sm_logger?.LogDebug(eventId, message, args);
        }
        public static void LogDebug(Exception exception, string message, params object[] args)
        {
            sm_logger?.LogDebug(exception, message, args);
        }
        public static void LogDebug(string message, params object[] args)
        {
            sm_logger?.LogDebug(message, args);
        }
        public static void LogError(string message, params object[] args)
        {
            sm_logger?.LogError(message, args);
        }
        public static void LogError(Exception exception, string message, params object[] args)
        {
            sm_logger?.LogError(exception, message, args);
        }
        public static void LogError(EventId eventId, Exception exception, string message, params object[] args)
        {
            sm_logger?.LogError(eventId, exception, message, args);
        }
        public static void LogError(EventId eventId, string message, params object[] args)
        {
            sm_logger?.LogError(eventId, message, args);
        }
        public static void LogInformation(EventId eventId, string message, params object[] args)
        {
            sm_logger?.LogInformation(eventId, message, args);
        }
        public static void LogInformation(Exception exception, string message, params object[] args)
        {
            sm_logger?.LogInformation(exception, message, args);
        }
        public static void LogInformation(EventId eventId, Exception exception, string message, params object[] args)
        {
            sm_logger?.LogInformation(eventId, exception, message, args);
        }
        public static void LogInformation(string message, params object[] args)
        {
            sm_logger?.LogInformation(message, args);
        }
        public static void LogTrace(string message, params object[] args)
        {
            sm_logger?.LogTrace(message, args);
        }
        public static void LogTrace(Exception exception, string message, params object[] args)
        {
            sm_logger?.LogTrace(exception, message, args);
        }
        public static void LogTrace(EventId eventId, string message, params object[] args)
        {
            sm_logger?.LogTrace(eventId, message, args);
        }
        public static void LogTrace(EventId eventId, Exception exception, string message, params object[] args)
        {
            sm_logger?.LogTrace(eventId, exception, message, args);
        }
        public static void LogWarning(EventId eventId, string message, params object[] args)
        {
            sm_logger?.LogWarning(eventId, message, args);
        }
        public static void LogWarning(EventId eventId, Exception exception, string message, params object[] args)
        {
            sm_logger?.LogWarning(eventId, exception, message, args);
        }
        public static void LogWarning(string message, params object[] args)
        {
            sm_logger?.LogWarning(message, args);
        }
        public static void LogWarning(Exception exception, string message, params object[] args)
        {
            sm_logger?.LogWarning(exception, message, args);
        }
    }
}
