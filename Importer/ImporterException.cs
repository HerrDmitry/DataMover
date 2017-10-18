using System;

namespace Importer
{
	public class ImporterException:Exception
	{
		private static Interfaces.ILog logger;

		public static void ConfigureLogger(Interfaces.ILog logger)
		{
			ImporterException.logger = logger;
		}

		public ImporterException(string message) : base(message)
		{
			logger?.Error(message);
		}
	}

	public class ImporterArgumentOutOfRangeException : ImporterException
	{
		public ImporterArgumentOutOfRangeException(string message) : base(message)
		{
		}
	}
	
	public class ImporterUploadException : ImporterException
	{
		public ImporterUploadException(string message) : base(message)
		{
		}
	}
}