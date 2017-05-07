using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace MKVCleaver2
{
	public class Track
	{
		public string Number { get; set; }
		public string UID { get; set; }
		public string Type { get; set; }
		public string Codec { get; set; }
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

		public EbmlElement this[string name]
		{
			get
			{
				foreach (var element in Elements)
				{
					if (element.Name == name)
						return element;
				}
				return null;
			}
		}
	}

	public class EbmlParser
	{
		public List<Track> Parse(string input)
		{
			input = input.Replace("|", "\n|");
			int startIndex = 1;
			while (true)
			{
				startIndex = input.IndexOf("+", startIndex);
				if (startIndex == -1)
					break;

				if (input[startIndex - 1] != ' ' && input[startIndex - 1] != '|')
				{
					input = input.Insert(startIndex, "\n");
					startIndex += 2;
				}
				else
					startIndex++;
			}

			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(input);
			writer.Flush();
			stream.Position = 0;

			List<EbmlElement> nodes = new List<EbmlElement>();
			using (var streamReader = new StreamReader(stream))
			{
				while (!streamReader.EndOfStream)
				{
					var line = streamReader.ReadLine();
					var clearLine = line.Replace("+", "").Replace("|", "").Trim();

					EbmlElement newElem;
					if (clearLine.Contains(":"))
					{
						var property = new Property();
						property.Name = clearLine.Split(':')[0].Trim();
						property.Value = clearLine.Split(':')[1].Trim();
						newElem = property;
					}
					else
					{
						var container = new Container();
						container.Name = clearLine;
						container.Elements = new List<EbmlElement>();
						newElem = container;
					}

					if (line.Contains("|"))
					{
						var hierarcyLevel = line.IndexOf("+") - 1;

						var parent = nodes.Last();
						for (int i = 0; i < hierarcyLevel; i++)
						{
							if (parent is Container)
							{
								Container temp = (Container) parent;

								int index = temp.Elements.Count - 1;
								while (!(temp.Elements[index] is Container))
								{
									index--;
								}
								parent = temp.Elements[index];
							}
						}

						((Container) parent).Elements.Add(newElem);
					}
					else
					{
						nodes.Add(newElem);
					}
				}
			}

			List<Track> tracks = new List<Track>();
			var segment = nodes.FirstOrDefault(x => x.Name.ToLower().Contains("сегмент"));
			if (segment != null)
			{
				var temp = (Container) segment;
				var tracksContainer = (Container)temp["Дорожки сегмента"];
				foreach (Container trackContainer in tracksContainer.Elements)
				{
					Track track = new Track();
					track.Number = ((Property) trackContainer["Номер дорожки"])?.Value;
					track.UID = ((Property)trackContainer["UID дорожки"])?.Value;
					track.Type = ((Property)trackContainer["Тип дорожки"])?.Value;
					track.Codec = ((Property)trackContainer["Идентификатор кодека"]).Value;
					track.Language = ((Property)trackContainer["Язык"])?.Value;
					track.Name = ((Property)trackContainer["Имя"])?.Value;

					tracks.Add(track);
				}
			}

			return tracks;
		}
	}
}
