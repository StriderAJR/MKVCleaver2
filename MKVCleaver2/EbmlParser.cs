using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace MKVCleaver2
{
	public class MkvFile : INotifyPropertyChanged
	{
		public string Path { get; set; }
		public string Name { get; set; }
		public List<Track> Tracks { get; set; }

		private bool _isSelected;
		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				_isSelected = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsSelected"));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}


	public class Track : INotifyPropertyChanged
	{
		public int Number { get; set; }
		public string UID { get; set; }
		public string Type { get; set; }
		public string Codec { get; set; }
		public string Language { get; set; }
		public string Name { get; set; }

		public MkvFile Parent { get; set; }

		private bool _isSelected;
		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				_isSelected = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsSelected"));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}

	public class BatchTrack
	{
		public string Name { get; set; }
		public Track Track { get; set; }

		private bool _isSelected;
		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				_isSelected = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsSelected"));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}

	public class TrackComparer : IEqualityComparer<Track>
	{
		public bool Equals(Track x, Track y)
		{
			return 
				x.Name == y.Name && 
				x.Codec == y.Codec && 
				x.Type == y.Type && 
				x.Language == y.Language && 
				x.Number == y.Number;
		}

		public int GetHashCode(Track x)
		{
			return (x.Name + x.Codec + x.Type + x.Language + x.Number).GetHashCode();
		}
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
						property.Name = clearLine.Split(new []{':'}, 2)[0].Trim();
						property.Value = clearLine.Split(new[] { ':' }, 2)[1].Trim();
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
			var segment = nodes.FirstOrDefault(x => x.Name.ToLower().Contains("segment"));
			if (segment != null)
			{
				var temp = (Container) segment;
				var tracksContainer = (Container)temp["Segment tracks"];
				foreach (Container trackContainer in tracksContainer.Elements)
				{
					Track track = new Track();
					track.Number = int.Parse(((Property) trackContainer["Track number"])?.Value.Split(' ')[0])-1;
					track.UID = ((Property)trackContainer["Track UID"])?.Value;
					track.Type = ((Property)trackContainer["Track type"])?.Value;
					track.Codec = ((Property)trackContainer["Codec ID"]).Value;
					track.Language = ((Property)trackContainer["Language"])?.Value;
					track.Name = ((Property)trackContainer["Name"])?.Value;

					tracks.Add(track);
				}
			}

			return tracks.OrderBy(x => x.Number).ToList();
		}
	}
}
