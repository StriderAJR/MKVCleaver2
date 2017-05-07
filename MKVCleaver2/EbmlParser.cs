using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace MKVCleaver2
{
	public enum TrackType
	{
		Video, Audio, Subtitle
	}

	public class Track
	{
		public uint Number { get; set; }
		public string UID { get; set; }
		public TrackType Type { get; set; }
		public string Language { get; set; }
		public string Name { get; set; }
	}

	public class EbmlElement
	{
		public string Name { get; set; }
	}

	public class Property : EbmlElement
	{
		public string Value { get; set; }
	}

	public class Container : EbmlElement
	{
		public List<EbmlElement> Elements { get; set; }
	}

	public class EbmlParser
	{
		public List<Track> Parse(string input)
		{
			input = input.Replace("|", "\n|");

			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(input);
			writer.Flush();
			stream.Position = 0;

			using (var streamReader = new StreamReader(stream))
			{
				var line = streamReader.ReadLine();
			}

			return null;
		}
	}
}
