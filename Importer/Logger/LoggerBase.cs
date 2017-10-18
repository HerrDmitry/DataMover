using System;
using Interfaces;

namespace Importer
{
	public abstract class LoggerBase:Interfaces.ILog
	{
		public Action<string> Info => message => this.WriteLine(LogLevel.Info, message);
		public Action<string> Debug => message => this.WriteLine(LogLevel.Debug, message);
		public Action<string> Warning => message => this.WriteLine(LogLevel.Warning, message);
		public Action<string> Error => message => this.WriteLine(LogLevel.Error, message);
		public Action<string> Fatal => message => this.WriteLine(LogLevel.Fatal, message);
		public abstract Action Terminate { get; }

		private static object logLocker=new object();

		protected LoggerBase():this(LogLevel.Debug)
		{
		}

		protected LoggerBase(LogLevel logLevel)
		{
			this.logLevel = logLevel;
		}

		protected abstract void WriteLog(LogLevel level, string message);

		private readonly LogLevel logLevel;
		private void WriteLine(LogLevel level, string message)
		{
			if (level < logLevel)
			{
				return;
			}
			lock (logLocker)
			{
				this.WriteLog(level, message);
			}
		}

	}
}