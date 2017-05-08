using System.Collections.Generic;
using System.IO;
using System.Text;
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

		public static string GetMkvExtractPath()
		{
			return iniData["General"]["ToolnixPath"] + "\\" + "mkvextract.exe";
		}

		public static string GetMkvExtractString(MkvFile mkvFile)
		{
			string filePath = mkvFile.Path;
			var tracks = mkvFile.Tracks;

			return GetMkvExtractString(filePath, tracks.ToArray());
		}

		public static string GetMkvExtractString(string filePath, params Track[] tracks)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("\"" + GetMkvExtractPath() + "\" --ui-language en ");
			sb.Append("tracks " + "\"" + filePath + "\" ");
			for (int i = 0; i < tracks.Length; i++)
			{
				sb.Append(tracks[i].Number + ":" + "\"" + GetTrackFileName(tracks[i]) + GetCodecContainerExtension(tracks[i].Codec) + "\" ");
			}

			return sb.ToString();
		}

		public static string GetTrackFileName(Track track)
		{
			return $"{track.Parent.Name}_Track{track.Number}";
		}

		public static string GetCodecContainerExtension(string codecId)
		{
			switch (codecId)
			{

				case "V_MPEG4/ISO/AVC":
					return ".h264";
				case "A_AAC":
					return ".aac";
				case "S_TEXT/ASS":
					return ".ass";
				case "S_TEXT/SSA":
					return ".ssa";
				default:
					return "";
			}
		}
	}
}
