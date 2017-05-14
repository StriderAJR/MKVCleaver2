using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using NEbml.Core;
using CheckBox = System.Windows.Controls.CheckBox;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace MKVCleaver2
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private List<String> _extractCommands = new List<String>();

		public MainWindow()
		{
			InitializeComponent();

			SettingsHelper.Init();
			if (String.IsNullOrEmpty(SettingsHelper.GetToolnixPath()))
			{
				MessageBoxResult result = MessageBox.Show(this, 
					"Don't forget to specify MKVToolnix path before extracting anything.",
					"Warning",
					MessageBoxButton.OK, 
					MessageBoxImage.Warning);
			}
		}

		#region Non-Control Methods

		private void ToggleButtonState()
		{
			if (tvFiles.Items.Count > 0)
			{
				btnRemoveFile.IsEnabled = true;
				btnExtract.IsEnabled = true;
			}
			if (tvFiles.Items.Count == 0)
			{
				btnRemoveFile.IsEnabled = false;
				btnExtract.IsEnabled = false;
			}
		}

		#endregion

		private void btnAddFile_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Multiselect = true;
			dialog.Filter = "MKV Files (*.mkv)|*.mkv";

			ObservableCollection<MkvFile> items = new ObservableCollection<MkvFile>();
			List<Track> intersection = new List<Track>();

			if (dialog.ShowDialog() == true)
			{
				foreach (var fileName in dialog.FileNames)
				{
					// start process
					var proc = new Process
					{
						StartInfo = new ProcessStartInfo
						{
							FileName = SettingsHelper.GetMkvInfoPath(),
							Arguments = "--ui-language en \"" + fileName + "\"",
							UseShellExecute = false,
							RedirectStandardOutput = true,
							CreateNoWindow = true
						}
					};

					proc.StartInfo.StandardOutputEncoding = Encoding.UTF8;

					proc.Start();

					String output = null;

					// get output
					while (!proc.StandardOutput.EndOfStream)
					{
						output += proc.StandardOutput.ReadLine();
					}

					proc.Close();

					var item = new MkvFile();
					item.Path = fileName;
					item.Name = Path.GetFileName(fileName);
					item.Tracks = new EbmlParser().Parse(output);
					item.Tracks.ForEach(x => x.Parent = item);
					items.Add(item);
				}
			}

			intersection.AddRange(items.First().Tracks);
			foreach (var mkvFile in items)
			{
				intersection = intersection.Intersect(mkvFile.Tracks, new TrackComparer()).ToList();
			}

			tvFiles.ItemsSource = items;

			ObservableCollection<BatchTrack> batchTracks = new ObservableCollection<BatchTrack>();
			foreach (var track in intersection)
			{
				batchTracks.Add(new BatchTrack
				{
					Name = string.Format("{0}, {1} ({2})", track.Type, track.Language, track.Name),
					Track = track
				});
			}
			
			lbBatchTracksToExtract.ItemsSource = batchTracks;
			
			ToggleButtonState();
		}

		private void btnRemoveFile_Click(object sender, RoutedEventArgs e)
		{
			tvFiles.Items.Remove(tvFiles.SelectedItem);
			ToggleButtonState();
		}

		private void btnLocateToolnix_Click(object sender, RoutedEventArgs e)
		{
			using (var dialog = new FolderBrowserDialog())
			{
				DialogResult result = dialog.ShowDialog();
				if (result == System.Windows.Forms.DialogResult.OK)
				{
					var selectedPath = dialog.SelectedPath;
					if (File.Exists(selectedPath + "\\" + "mkvinfo.exe") &&
					    File.Exists(selectedPath + "\\" + "mkvextract.exe"))
					{
						SettingsHelper.SetToolnixPath(selectedPath);
					}
					else
					{
						MessageBoxResult message = MessageBox.Show(this,
							"Selected folder doesn't contain MKVToolnix utils! Please, specify the correct MKVToolnix folder",
							"Wrong",
							MessageBoxButton.OK,
							MessageBoxImage.Exclamation);
					}
				}
			}
		}

		private void tbExtractCommand_Refresh()
		{
			tbExtractCommand.Clear();
			foreach (var str in _extractCommands)
			{
				tbExtractCommand.Text += str + "\n";
			}
		}

		private void cbIsFileSelected_Checked(object sender, RoutedEventArgs e)
		{
			var checkBox = (CheckBox) sender;
			var mkvFile = (MkvFile) checkBox.DataContext;
			mkvFile.IsSelected = true;
			mkvFile.Tracks.ForEach(x => x.IsSelected = true);
		}

		private void cbIsFileSelected_Unchecked(object sender, RoutedEventArgs e)
		{
			var checkBox = (CheckBox)sender;
			var mkvFile = (MkvFile)checkBox.DataContext;
			mkvFile.IsSelected = false;
			mkvFile.Tracks.ForEach(x => x.IsSelected = false);
		}

		private void cbIsSelected_Checked(object sender, RoutedEventArgs e)
		{
			var checkBox = (CheckBox)sender;
			var track = (Track)checkBox.DataContext;
			var mkvFile = track.Parent;
			track.IsSelected = true;

			bool isAllSelected = true;
			foreach (var mkvTrack in mkvFile.Tracks)
			{
				if (!mkvTrack.IsSelected)
					isAllSelected = false;
			}
			if (isAllSelected)
				mkvFile.IsSelected = true;
		}

		private void cbIsSelected_Unchecked(object sender, RoutedEventArgs e)
		{
			var checkBox = (CheckBox)sender;
			var track = (Track)checkBox.DataContext;
			var mkvFile = track.Parent;
			track.IsSelected = false;
			mkvFile.IsSelected = false;
		}

		private void cbBatchTrackIsSelected_Checked(object sender, RoutedEventArgs e)
		{
			
		}

		private void cbBatchTrackIsSelected_Unchecked(object sender, RoutedEventArgs e)
		{
			
		}
	}
}
