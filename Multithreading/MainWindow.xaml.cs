using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Multithreading.ViewModels;

namespace Multithreading
{
    public partial class MainWindow
    {
        private readonly NewDownloadViewModel _downloadViewModel;
        private readonly QueueManagerViewModel _queueManagerViewModel;
        private readonly DownloadManager _downloadManager;

        public MainWindow()
        {
            _downloadManager = DownloadManager.GetInstance();
            var viewModel = new MainWindowViewModel();
            _queueManagerViewModel = new QueueManagerViewModel();
            _downloadViewModel = new NewDownloadViewModel();
            DataContext = viewModel;
            InitializeComponent();
        }

        private void NewDownloadCommand_OnClick(object sender, RoutedEventArgs e)
        {
            var newDownloadWindow = new NewDownload(_downloadViewModel);
            if (newDownloadWindow.ShowDialog() == true)
            {
                var download = DownloadManager.CreateDownload(
                    newDownloadWindow.FilePath.Text,
                    newDownloadWindow.DestinationPath.Text,
                    _downloadViewModel.SelectedQueue.Model);
                if (download.HasError)
                {
                    Dispatcher.BeginInvoke((Action)delegate
                    {
                        MessageBox.Show(download.ErrorMessage, "Download error", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                    return;
                }

                _downloadManager.AddDownload(download);
            }
        }

        private void Download_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var rowControl = e.OriginalSource as FrameworkElement;
            var download = rowControl?.DataContext as DownloadViewModel;
            if (download != null)
            {
                Process.Start(download.DestinationPath);
            }
        }

        private void CancelDownloadCommand_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var download = button?.DataContext as DownloadViewModel;
            _downloadManager.CancelDownload(download?.Model);
            download?.Dispose();
        }

        private void QueueManagerCommand_OnClick(object sender, RoutedEventArgs e)
        {
            var queueManagerWindow = new QueueManager(_queueManagerViewModel);
            queueManagerWindow.ShowDialog();
        }
    }
}
