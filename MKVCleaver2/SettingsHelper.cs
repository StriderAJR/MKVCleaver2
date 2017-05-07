using System.IO;
using IniParser;
using IniParser.Model;

namespace MKVCleaver2
{
	static class SettingsHelper
	{
		private const string settingsFileName = "settings.ini";
		private static FileIniDataParser parser = new FileIniDataParser();
		private static IniData iniData;

		public static void Init()
		{
			if (File.Exists(settingsFileName))
			{
				iniData = parser.ReadFile(settingsFileName);
			}
			else
			{
				File.Create(settingsFileName);
				iniData = new IniData();
			}
		}

		public static void SetToolnixPath(string path)
		{
			iniData["General"]["ToolnixPath"] = path;
			parser.WriteFile(settingsFileName, iniData);
		}

		public static string GetToolnixPath()
		{
			return iniData["General"]["ToolnixPath"];
		}

		public static string GetMkvInfoPath()
		{
			return iniData["General"]["ToolnixPath"] + "\\" + "mkvinfo.exe";
		}
	}
}
