using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Input;
using Multithreading.ViewModels;

namespace Multithreading
{
    public partial class MainWindow
    {
        private readonly MainWindowViewModel _viewModel;

        public MainWindow()
        {
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;
            InitializeComponent();
        }

        private void NewDownloadCommand_OnClick(object sender, RoutedEventArgs e)
        {
            var newDownloadWindow = new NewDownload();
            if (newDownloadWindow.ShowDialog() == true)
            {
                var download = DownloadManager.CreateDownload(newDownloadWindow.FilePath.Text, newDownloadWindow.DestinationPath.Text);
                var downloadViewModel = new DownloadViewModel(download);
                _viewModel.Downloads.Add(downloadViewModel);
                if (download.HasError)
                {
                    Dispatcher.BeginInvoke((Action)delegate
                    {
                        MessageBox.Show(download.ErrorMessage, "Download error", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                    return;
                }

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
            if (e.Error != null)
            {
                var download = e.UserState as DownloadViewModel;
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
    }
}
