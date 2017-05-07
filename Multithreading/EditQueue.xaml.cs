using System.Windows;
using Multithreading.Models;
using Multithreading.ViewModels;

namespace Multithreading
{
    public partial class EditQueue : Window
    {
        private readonly QueueViewModel _originlModel;

        public EditQueue(QueueViewModel queueForUpdate = null)
        {
            InitializeComponent();
            _originlModel = queueForUpdate;
            DataContext = queueForUpdate != null
                ? new EditQueueViewModel(queueForUpdate.Model.Clone())
                : new EditQueueViewModel();
            Title = queueForUpdate == null
                ? "Create queue"
                : "Update queue";
        }

        public Queue CurrentQueue => (DataContext as EditQueueViewModel)?.Model;

        private void SaveQueueCommand_Click(object sender, RoutedEventArgs e)
        {
            var model = (DataContext as EditQueueViewModel);
            if (model != null && _originlModel != null)
            {
                _originlModel.Model.Type = model.Model.Type;
            }
            DialogResult = true;
            Close();
        }
    }
}
