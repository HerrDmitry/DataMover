using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;

namespace Importer
{
	public class FileLogger:LoggerBase
	{
		private StreamWriter stream;
		private Action<string> writer;
		private ConcurrentQueue<string> queue;
		private Task writerTask;
		private bool isTerminating;
		public FileLogger(string path, LogLevel logLevel = LogLevel.Debug) : base(logLevel)
		{
			try
			{
				if (!File.Exists(path))
				{
					stream = new StreamWriter(File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read));
				}
				else
				{
					stream = new StreamWriter(File.Open(path, FileMode.Truncate, FileAccess.Write, FileShare.Read));
				}
				queue = new ConcurrentQueue<string>();
				writerTask = new Task(() =>
				{
					bool needFlush = false;
					while (true)
					{
						if (queue.TryDequeue(out string msg))
						{
							stream?.WriteLineAsync(msg).Wait();
							needFlush = true;
						}
						else
						{
							if (needFlush)
							{
								stream?.FlushAsync().Wait();
								needFlush = false;
							}
							if (isTerminating)
							{
								return;
							}
							Thread.Sleep(10);
						}
					}
				});
				writerTask.Start();
				writer = GetFileWriter(stream,queue);
				Terminate = () =>
				{
					isTerminating = true;
					writerTask?.Wait();
				};
			}
			catch
			{
				stream = null;
			}
		}

		public override Action Terminate { get; }

		protected override void WriteLog(LogLevel level, string message)
		{
			writer($"{DateTime.Now:G} - {level.ToString()} - {message}");
		}

		private static Action<string> GetFileWriter(StreamWriter stream, ConcurrentQueue<string> queue)
		{
			return msg => { queue.Enqueue(msg); };
		}
	}
}