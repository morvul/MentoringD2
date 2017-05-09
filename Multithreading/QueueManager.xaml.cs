using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Multithreading.ViewModels;

namespace Multithreading
{
    public partial class QueueManager
    {
        private readonly DownloadManager _downloadManager;

        public QueueManager(QueueManagerViewModel viewModel)
        {
            DataContext = viewModel;
            _downloadManager = DownloadManager.GetInstance();
            InitializeComponent();
        }

        private void Queue_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var rowControl = e.OriginalSource as FrameworkElement;
            var queueViewModel = rowControl?.DataContext as QueueViewModel;
            if (queueViewModel != null)
            {
                var queueEditWindow = new EditQueue(queueViewModel);
                queueEditWindow.ShowDialog();
            }
        }

        private void CancelQueueCommand_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var download = button?.DataContext as QueueViewModel;
            _downloadManager.AbortQueue(download?.Model);
        }

        private void NewQueueCommand_OnClick(object sender, RoutedEventArgs e)
        {
            var newQueueWindow = new EditQueue();
            if (newQueueWindow.ShowDialog() == true)
            {
                _downloadManager.CreateQueue(newQueueWindow.CurrentQueue);
            }
        }

        private void StopQueueCommand_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var download = button?.DataContext as QueueViewModel;
            _downloadManager.StopQueue(download?.Model);
        }
    }
}
