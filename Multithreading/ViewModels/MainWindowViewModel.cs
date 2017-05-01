using System.Collections.ObjectModel;

namespace Multithreading.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ObservableCollection<DownloadViewModel> _downloads;

        public MainWindowViewModel()
        {
            Downloads = new ObservableCollection<DownloadViewModel>();
        }
        
        public ObservableCollection<DownloadViewModel> Downloads
        {
            get { return _downloads; }
            private set
            {
                if (_downloads != value)
                {
                    _downloads = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
