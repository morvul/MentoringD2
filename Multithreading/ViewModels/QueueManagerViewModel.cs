using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Multithreading.ViewModels
{
    public class QueueManagerViewModel : ViewModelBase<object>
    {
        private readonly DownloadManager _downloadManager;
        private ObservableCollection<QueueViewModel> _queues;

        public QueueManagerViewModel()
            : base(null)
        {
            _downloadManager = DownloadManager.GetInstance();
            _downloadManager.Queues.CollectionChanged += QueueListUpdated;
            UpdateQueueList();
        }

        private void QueueListUpdated(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateQueueList();
        }

        private void UpdateQueueList()
        {
            var queues = _downloadManager.Queues.Select(x => new QueueViewModel(x));
            Queues = new ObservableCollection<QueueViewModel>(queues);
        }

        public ObservableCollection<QueueViewModel> Queues
        {
            get { return _queues; }
            private set
            {
                if (_queues != value)
                {
                    _queues = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
