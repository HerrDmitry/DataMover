using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using log4net.Core;

namespace DataMover.Logger
{
    public static class DataMoverLog
    {
        private static ConcurrentQueue<LogMessage> _logQueue = new ConcurrentQueue<LogMessage>();

        static DataMoverLog()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            if (File.Exists("log4net.config"))
            {
                XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
            }
            
            _token=new CancellationTokenSource();
            _loggerTask=Task.Run(()=>LoggerTask(_token.Token));
            DebugAsync("Logging is initialized");
        }

        public static void ErrorAsync(string message)
        {
            _logQueue.Enqueue(new LogMessage {Type = LogMessageType.Error, Message = message});
        }

        public static void WairningAsync(string message)
        {
            _logQueue.Enqueue(new LogMessage {Type = LogMessageType.Warning, Message = message});
        }

        public static void InfoAsync(string message)
        {
            _logQueue.Enqueue(new LogMessage {Type = LogMessageType.Info, Message = message});
        }

        public static void DebugAsync(string message)
        {
            _logQueue.Enqueue(new LogMessage {Type = LogMessageType.Debug, Message = message});
        }

        public static void Terminate()
        {
            InfoAsync("End of log");
            _token?.Cancel();
            _loggerTask?.Wait(500);
            _loggerTask = null;
            _token = null;
            _logQueue = null;
        }

        private static void LoggerTask(CancellationToken token)
        {
            var logger = LogManager.GetLogger(typeof(DataMoverLog));
            while (!token.IsCancellationRequested)
            {
                while (_logQueue.TryDequeue(out LogMessage message))
                {
                    switch (message.Type)
                    {
                        case LogMessageType.Error:
                            logger.Error(message.Message);
                            break;
                        case LogMessageType.Warning:
                            logger.Warn(message.Message);
                            break;
                        case LogMessageType.Info:
                            logger.Info(message.Message);
                            break;
                        default:
                            logger.Debug(message.Message);
                            break;
                    }
                }

                if (!token.IsCancellationRequested)
                {
                    Thread.Sleep(100);
                }
            }
        }

        private static Task _loggerTask;
        private static CancellationTokenSource _token;

        private struct LogMessage
        {
            public LogMessageType Type;
            public string Message;
        }

        private enum LogMessageType
        {
            Error,
            Warning,
            Info,
            Debug
        }
    }
}