using System;
using System.IO;
using Interfaces.Configuration;

namespace Importer.Writers
{
	public static partial class Writers
	{
		private class WaveStream : Stream
		{
			private Interfaces.ILog logger;
			private IFile fileConfig;
			private Func<string> metadataBuilder;
			private MemoryStream ms;
			private readonly Func<MemoryStream, bool, int> writeToWaveFunc;
			private long recordCount;
			public WaveStream(IFile fileConfig, Interfaces.ILog log)
			{
				this.logger = log;
				this.fileConfig = fileConfig;
				this.metadataBuilder = fileConfig.GetMetadataBuilder();
				this.ms=new MemoryStream();
				this.writeToWaveFunc = fileConfig.GetDataSenderFunc(log).Result;
			}

			public override void Flush()
			{
				this.writeToWaveFunc(ms, true);
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				throw new NotImplementedException();
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				throw new NotImplementedException();
			}

			public override void SetLength(long value)
			{
				throw new NotImplementedException();
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				ms.Write(buffer, offset, count);
				if (ms.Length >= 1024 * 1024 * 9.9)
				{
					this.writeToWaveFunc(ms, false);
					ms=new MemoryStream();
				}
			}

			public override bool CanRead => false;
			public override bool CanSeek => false;
			public override bool CanWrite => true;
			public override long Length { get; }
			public override long Position { get; set; }
		}
	}
}