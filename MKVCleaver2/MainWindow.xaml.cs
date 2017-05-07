using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using NEbml.Core;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace MKVCleaver2
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
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

		private void btnAddFile_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Multiselect = true;
			dialog.Filter = "MKV Files (*.mkv)|*.mkv";
			if (dialog.ShowDialog() == true)
			{
				foreach (var fileName in dialog.FileNames)
				{
					tvFiles.Items.Add(fileName);

					// start process
					var proc = new Process
					{
						StartInfo = new ProcessStartInfo
						{
							FileName = SettingsHelper.GetMkvInfoPath(),
							Arguments = "\"" + fileName + "\"",
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

					new EbmlParser().Parse(output);
				}
			}

			if (tvFiles.Items.Count > 0)
				btnRemoveFile.IsEnabled = true;
		}

		private void btnRemoveFile_Click(object sender, RoutedEventArgs e)
		{
			tvFiles.Items.Remove(tvFiles.SelectedItem);
			if (tvFiles.Items.Count == 0)
				btnRemoveFile.IsEnabled = false;
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
	}
}
