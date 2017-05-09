using Multithreading.Enums;
using Multithreading.Models;

namespace Multithreading.ViewModels
{
    public class QueueViewModel : ViewModelBase<Queue>
    {

        public QueueViewModel()
            : base(new Queue())
        {
            Model.IsInProgressChanged += IsInProgressChanged;
        }

        public QueueViewModel(string queueName)
            : base(new Queue())
        {
            Model.Description = queueName;
            Model.IsInProgressChanged += IsInProgressChanged;
        }

        public QueueViewModel(Queue queue)
            : base(queue)
        {
            Model.IsInProgressChanged += IsInProgressChanged;
        }
        ~QueueViewModel()

        {
            Model.IsInProgressChanged -= IsInProgressChanged;
        }

        public int Number
        {
            get { return Model.Number; }
            set
            {
                if (Model.Number != value)
                {
                    Model.Number = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string Name => Model.Name;

        public QueueType Type => Model.Type;

        public bool IsInProgress => Model.IsInProgress;

        public bool IsDefault => Model.Number == Queue.DefaultNumber;

        public override void Refresh()
        {
            base.Refresh();
            Model.UpdateQueueState();
        }

        private void IsInProgressChanged()
        {
            OnPropertyChanged(nameof(IsInProgress));
        }
    }
}
