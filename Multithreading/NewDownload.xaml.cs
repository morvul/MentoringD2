using System.Windows;
using System.Windows.Forms;
using Clipboard = System.Windows.Clipboard;

namespace Multithreading
{
    public partial class NewDownload : Window
    {
        public NewDownload()
        {
            InitializeComponent();
            DestinationPath.Text = KnownFolders.GetPath(KnownFolder.Downloads);
            FilePath.Text = Clipboard.GetText();
        }

        private void CreateDownloadCommand_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void DestinationCommand_Click(object sender, RoutedEventArgs e)
        {
            using (var directoryPicker = new FolderBrowserDialog())
            {
                var result = directoryPicker.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    DestinationPath.Text = directoryPicker.SelectedPath;
                }
            }
        }

        private void LocalFileCommand_Click(object sender, RoutedEventArgs e)
        {
            using (var directoryPicker = new OpenFileDialog())
            {
                var result = directoryPicker.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    FilePath.Text = directoryPicker.FileName;
                }
            }
        }
    }
}
