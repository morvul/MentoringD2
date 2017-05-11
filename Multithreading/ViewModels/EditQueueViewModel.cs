using System;
using System.Collections.ObjectModel;
using Multithreading.Enums;
using Multithreading.Models;

namespace Multithreading.ViewModels
{
    public class EditQueueViewModel : ViewModelBase<Queue>
    {
        private ObservableCollection<QueueType> _queueTypes;

        public EditQueueViewModel()
            : this(new Queue())
        {
        }

        public EditQueueViewModel(Queue queue)
            : base(queue)
        {
            QueueTypes = new ObservableCollection<QueueType>
            {
                QueueType.Parallel,
                QueueType.Sequence
            };
        }

        public EditQueueViewModel(QueueViewModel queueViewModel)
            : this(queueViewModel.Model)
        {
        }

        public ObservableCollection<QueueType> QueueTypes
        {
            get { return _queueTypes; }
            set
            {
                if (_queueTypes != value)
                {
                    _queueTypes = value;
                    OnPropertyChanged();
                }
            }
        }

        public QueueType SelectedQueueType
        {
            get { return Model.Type; }
            set
            {
                if (Model.Type != value)
                {
                    Model.Type = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime StartTime
        {
            get { return Model.StartTime; }
            set
            {
                if (Model.StartTime != value)
                {
                    Model.StartTime = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
