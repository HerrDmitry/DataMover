using System;
using System.Collections.Generic;
using Interfaces;

namespace Importer
{
	public class MultiLogger:LoggerBase
	{
		public MultiLogger()
		{
			Terminate = () => { loggers.ForEach(x => x.Terminate()); };
		}

		private List<Interfaces.ILog> loggers=new List<ILog>();
		public override Action Terminate { get; }

		protected override void WriteLog(LogLevel level, string message)
		{
			loggers.ForEach(x =>
			{
				switch (level)
				{
					case LogLevel.Debug:
						x.Debug(message);
						break;
					case LogLevel.Info:
						x.Info(message);
						break;
					case LogLevel.Warning:
						x.Warning(message);
						break;
					case LogLevel.Error:
						x.Error(message);
						break;
					case LogLevel.Fatal:
						x.Fatal(message);
						break;
				}
			});
		}

		public void AddLogger(Interfaces.ILog logger)
		{
			loggers.Add(logger);
		}
	}
}