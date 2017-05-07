using System.Collections.ObjectModel;

namespace Multithreading.ViewModels
{
    public class QueueManagerViewModel : ViewModelBase<object>
    {
        private ObservableCollection<QueueViewModel> _queues;
        private QueueViewModel _defaultQueue;

        public QueueManagerViewModel()
            : base(null)
        {
            Queues = new ObservableCollection<QueueViewModel>();
            DefaultQueue = new QueueViewModel("Default");
            Queues.Add(DefaultQueue);
        }
        
        public QueueViewModel DefaultQueue
        {
            get { return _defaultQueue; }
            set
            {
                if (_defaultQueue != value)
                {
                    _defaultQueue = value;
                    OnPropertyChanged();
                }
            }
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
