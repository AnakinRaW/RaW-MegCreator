using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Alamo.CLI;
using Microsoft.Win32;

namespace RaW_MegCreator
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CreateMeg(object sender, RoutedEventArgs e)
        {
            var meg = new MegaFile();

            if (!AddFiles(meg))
            {
                MessageBox.Show("Error Adding Files");
                return;
            }

            try
            {
                using (var stream = File.Open(FilePath.Text, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    meg.Close(stream, MegaFile.Format.V1, new MegaFile.EncryptionKey?());
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error writing .meg file");
            }

        }

        private bool AddFiles(MegaFile meg)
        {
            if (!Directory.Exists("Data"))
                return false;

            var files = Directory.EnumerateFiles("Data", "*.*", SearchOption.AllDirectories).Where(x =>
                x.IndexOf(@"\XML\", StringComparison.OrdinalIgnoreCase) >= 0
                || x.IndexOf(@"\SCRIPTS\", StringComparison.OrdinalIgnoreCase) >= 0
                || x.IndexOf(@"\CustomMaps\", StringComparison.OrdinalIgnoreCase) >= 0);
            foreach (var file in files)
            {
                var fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                meg.InsertFile(file, fs);
            }

            return true;
        }

        private void Browse(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.FileName = "AIFiles.meg";
            dialog.InitialDirectory = Assembly.GetEntryAssembly().Location;
            dialog.RestoreDirectory = true;
            dialog.Filter = "MEG File|*.meg";
            if (dialog.ShowDialog(this) == true)
            {
                FilePath.Text = dialog.FileName;
            }
        }
    }
}
