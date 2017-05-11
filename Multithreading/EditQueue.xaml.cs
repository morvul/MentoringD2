using System.Windows;
using Multithreading.Models;
using Multithreading.ViewModels;

namespace Multithreading
{
    public partial class EditQueue : Window
    {
        private readonly QueueViewModel _originlViewModel;

        public EditQueue(QueueViewModel queueForUpdate = null)
        {
            InitializeComponent();
            _originlViewModel = queueForUpdate;
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
            if (model != null && _originlViewModel != null)
            {
                _originlViewModel.Model.Type = model.Model.Type;
                _originlViewModel.Model.StartTime = model.Model.StartTime;
                _originlViewModel.Refresh();
            }
            DialogResult = true;
            Close();
        }
    }
}
