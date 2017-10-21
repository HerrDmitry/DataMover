using System;
using System.Collections.Generic;
using System.IO;

namespace Importer.Writers
{
	public class WriteStreamProxy:Stream,IDisposable
	{
		private List<Stream> underlyingStreams=new List<Stream>();

		public WriteStreamProxy()
		{
		}

		public WriteStreamProxy(Stream stream)
		{
			if (stream != null)
			{
				underlyingStreams.Add(stream);
			}
		}

		public WriteStreamProxy(IList<Stream> streams)
		{
			foreach (var stream in streams)
			{
				if (stream != null)
				{
					underlyingStreams.Add(stream);
				}
			}
		}

		public void AddStream(Stream stream)
		{
			underlyingStreams.Add(stream);
		}

		public override void Flush()
		{
			foreach (var stream in underlyingStreams)
			{
				stream.Flush();
			}
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
			foreach (var stream in underlyingStreams)
			{
				stream.Write(buffer,offset,count);
			}
		}

		public new void Dispose()
		{
			foreach (var stream in underlyingStreams)
			{
				stream.Dispose();
			}
			underlyingStreams.Clear();
		}

		public override bool CanRead => false;
		public override bool CanSeek => false;
		public override bool CanWrite => true;
		public override long Length => -1;
		public override long Position
		{
			get => -1;
			set => throw new NotImplementedException();
		}
	}
}