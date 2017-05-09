using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Multithreading.ViewModels;
using Clipboard = System.Windows.Clipboard;

namespace Multithreading
{
    public partial class NewDownload : Window
    {
        private readonly DownloadManager _downloadManager;

        public NewDownload(NewDownloadViewModel downloadViewModel)
        {
            InitializeComponent();
            _downloadManager = DownloadManager.GetInstance();
            DestinationPath.Text = KnownFolders.GetPath(KnownFolder.Downloads);
            var urlstr = Clipboard.GetText();
            if (Uri.IsWellFormedUriString(urlstr, UriKind.RelativeOrAbsolute))
            {
                FilePath.Text = urlstr;
            }

            DataContext = downloadViewModel;
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

        private void NewQueueCommand_Click(object sender, RoutedEventArgs e)
        {
            var newQueueWindow = new EditQueue();
            if (newQueueWindow.ShowDialog() == true)
            {
                var newDownloadViewModel = DataContext as NewDownloadViewModel;
                if (newDownloadViewModel != null)
                {
                    _downloadManager.CreateQueue(newQueueWindow.CurrentQueue);
                    var newQueueViewModel =
                        newDownloadViewModel.Queues.FirstOrDefault(x => x.Name == newQueueWindow.CurrentQueue.Name);
                    newDownloadViewModel.SelectedQueue = newQueueViewModel;
                }
            }
        }
    }
}
