using System.Collections.Concurrent;

namespace DataMover.Logger
{
    public static class DataMoverLog
    {
        private static ConcurrentQueue<LogMessage> _logQueue=new ConcurrentQueue<LogMessage>();
        public static void Error(string message)
        {
            _logQueue.Enqueue(new LogMessage{Type=LogMessageType.Error, Message = message});
        }

        public static void Wairning(string message)
        {
            _logQueue.Enqueue(new LogMessage{Type=LogMessageType.Warning, Message=message});
        }

        public static void Info(string message)
        {
            _logQueue.Enqueue(new LogMessage{Type=LogMessageType.Info, Message = message});
        }

        public static void Debug(string message)
        {
            _logQueue.Enqueue(new LogMessage{Type=LogMessageType.Debug, Message = message});
        }

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