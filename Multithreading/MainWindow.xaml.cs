using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Multithreading.ViewModels;

namespace Multithreading
{
    public partial class MainWindow
    {
        private readonly MainWindowViewModel _viewModel;
        private readonly NewDownloadViewModel _downloadViewModel;
        private readonly QueueManagerViewModel _queueManagerViewModel;

        public MainWindow()
        {
            _viewModel = new MainWindowViewModel();
            _queueManagerViewModel = new QueueManagerViewModel();
            _downloadViewModel = new NewDownloadViewModel(_queueManagerViewModel.Queues);
            DataContext = _viewModel;
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
                var downloadViewModel = new DownloadViewModel(download);
                if (download.HasError)
                {
                    Dispatcher.BeginInvoke((Action)delegate
                    {
                        MessageBox.Show(download.ErrorMessage, "Download error", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                    return;
                }

                _viewModel.Downloads.Add(downloadViewModel);
                download.WebClient.DownloadProgressChanged += client_DownloadProgressChanged;
                download.WebClient.DownloadFileCompleted += client_DownloadFileCompleted;
                DownloadManager.StartDownload(download, downloadViewModel);
            }
            
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var download = e.UserState as DownloadViewModel;
            if (download == null)
            {
                return;
            }

            Dispatcher.BeginInvoke((Action)delegate
            {
                download.Progress = e.ProgressPercentage;
            });
        }
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            var download = e.UserState as DownloadViewModel;
            if (e.Cancelled)
            {
                Dispatcher.BeginInvoke((Action) delegate
                {
                    _viewModel.Downloads.Remove(download);
                });
                return;
            }

            if (e.Error != null)
            {
                if (download != null)
                {
                    download.ErrorMessage = e.Error.InnerException?.Message ?? e.Error.Message;
                }

                Dispatcher.BeginInvoke((Action)delegate
                {
                    MessageBox.Show(e.Error.InnerException?.Message, "Download error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
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
            DownloadManager.AbortDownload(download?.Model);
        }

        private void QueueManagerCommand_OnClick(object sender, RoutedEventArgs e)
        {
            var queueManagerWindow = new QueueManager(_queueManagerViewModel);
            queueManagerWindow.ShowDialog();
        }
    }
}
