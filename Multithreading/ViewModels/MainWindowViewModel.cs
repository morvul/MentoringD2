using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Multithreading.ViewModels
{
    public class MainWindowViewModel : ViewModelBase<object>
    {
        private readonly DownloadManager _downloadManager;
        private ObservableCollection<DownloadViewModel> _downloads;

        public MainWindowViewModel()
            : base(null)
        {
            _downloadManager = DownloadManager.GetInstance();
            _downloadManager.Downloads.CollectionChanged += DownloadListUpdated;
            UpdateDownloads();
        }

        private void DownloadListUpdated(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateDownloads();
        }

        private void UpdateDownloads()
        {
            var downloads = _downloadManager.Downloads.Select(x => new DownloadViewModel(x));
            if (Downloads != null)
            {
                foreach (var downloadViewModel in Downloads)
                {
                    downloadViewModel.Dispose();
                }
            }

            Downloads = new ObservableCollection<DownloadViewModel>(downloads);
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
