using System.Collections.ObjectModel;
using System.Linq;
using Multithreading.Models;

namespace Multithreading.ViewModels
{
    public class NewDownloadViewModel : ViewModelBase<object>
    {
        private ObservableCollection<QueueViewModel> _queues;
        private QueueViewModel _selectedQueue;

        public NewDownloadViewModel(ObservableCollection<QueueViewModel> queues)
            : base(null)
        {
            Queues = queues;
            SelectedQueue = Queues.FirstOrDefault(x => x.Number == Queue.DefaultNumber);
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
