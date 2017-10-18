using System.Collections.Generic;
using System.ComponentModel.Design;

namespace Importer
{
	public static class Localization
	{
		private static Dictionary<string, string> localization;
		private static Dictionary<string, string> defaultLocalization;
		private static string language = "default";

		static Localization()
		{
			localization = new Dictionary<string, string>();
			defaultLocalization = new Dictionary<string, string>();
			LoadDefaults();
		}

		public static string GetLocalizationString(string key, params object[] values)
		{
			if (!localization.TryGetValue(key, out var localizedString))
			{
				if (!defaultLocalization.TryGetValue(key, out localizedString))
				{
					localizedString = key;
				}
			}

			return values?.Length > 0 ? string.Format(localizedString, values) : localizedString;
		}

		public static void SetLanguage(string lang)
		{
			language = lang;
			LoadLanguage();
		}

		private static void LoadLanguage()
		{
		}

		private static void LoadDefaults()
		{
			defaultLocalization["test"] = "test";
		}
	}
}