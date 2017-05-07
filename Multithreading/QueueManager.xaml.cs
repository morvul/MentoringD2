using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Multithreading.ViewModels;

namespace Multithreading
{
    public partial class QueueManager
    {
        public QueueManager(QueueManagerViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        private void Queue_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var rowControl = e.OriginalSource as FrameworkElement;
            var queue = rowControl?.DataContext as QueueViewModel;
            if (queue != null)
            {
                var queueEditWindow = new EditQueue(queue);
                queueEditWindow.ShowDialog();
            }
        }

        private void CancelQueueCommand_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var download = button?.DataContext as QueueViewModel;
            DownloadManager.AbortQueue(download?.Model);
        }

        private void NewQueueCommand_OnClick(object sender, RoutedEventArgs e)
        {
            var newQueueWindow = new EditQueue();
            if (newQueueWindow.ShowDialog() == true)
            {
                var newDownloadViewModel = DataContext as QueueManagerViewModel;
                if (newDownloadViewModel != null)
                {
                    var newQueueViewModel = new QueueViewModel(newQueueWindow.CurrentQueue);
                    newDownloadViewModel.Queues.Add(newQueueViewModel);
                    DownloadManager.UpdateQueuesNumbers(newDownloadViewModel.Queues);
                }
            }
        }

    }
}
