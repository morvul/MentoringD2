using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Multithreading.Models;

namespace Multithreading.ViewModels
{
    public class NewDownloadViewModel : ViewModelBase<object>
    {
        private readonly DownloadManager _downloadManager;
        private ObservableCollection<QueueViewModel> _queues;
        private QueueViewModel _selectedQueue;

        public NewDownloadViewModel()
            : base(null)
        {
            _downloadManager = DownloadManager.GetInstance();
            _downloadManager.Queues.CollectionChanged += QueueChanged;
            UpdateQueueList();
            SelectedQueue = Queues.FirstOrDefault(x => x.Number == Queue.DefaultNumber);
        }

        private void QueueChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateQueueList();
        }

        private void UpdateQueueList()
        {
            var selectedQueue = SelectedQueue;
            var queues = _downloadManager.Queues.Select(x => new QueueViewModel(x));
            Queues = new ObservableCollection<QueueViewModel>(queues);
            SelectedQueue = Queues.FirstOrDefault(x => x.Name == selectedQueue?.Name);
        }


        public ObservableCollection<QueueViewModel> Queues
        {
            get { return _queues; }
            set
            {
                if (_queues != value)
                {
                    _queues = value;
                    OnPropertyChanged();
                }
            }
        }

        public QueueViewModel SelectedQueue
        {
            get { return _selectedQueue; }
            set
            {
                if (_selectedQueue != value)
                {
                    _selectedQueue = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
