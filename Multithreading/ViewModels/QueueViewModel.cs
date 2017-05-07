using Multithreading.Models;

namespace Multithreading.ViewModels
{
    public class QueueViewModel : ViewModelBase<Queue>
    {

        public QueueViewModel()
            : base(new Queue())
        {
        }

        public QueueViewModel(string queueName)
            : base(new Queue())
        {
            Model.Description = queueName;
        }

        public QueueViewModel(Queue queue)
            : base(queue)
        {
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

        public bool IsInProgress => Model.IsInProgress;
    }
}
