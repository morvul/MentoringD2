using System;
using System.Net;
using Multithreading.Models;

namespace Multithreading.ViewModels
{
    public class DownloadViewModel : ViewModelBase
    {
        private readonly Download _download;

        public DownloadViewModel()
        {
            _download = new Download();
        }

        public DownloadViewModel(Download download)
        {
            _download = download;
        }

        public string FileName
        {
            get { return _download.FileName; }
            set
            {
                if (_download.FileName != value)
                {
                    _download.FileName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ErrorMessage
        {
            get { return _download.ErrorMessage; }
            set
            {
                if (_download.ErrorMessage != value)
                {
                    _download.ErrorMessage = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ProgressMessage));
                    OnPropertyChanged(nameof(IsInProgress));
                }
            }
        }

        public bool HasError => ErrorMessage != null;

        public bool IsInProgress => Progress < 100 && !HasError;

        public WebClient WebClient => _download.WebClient;

        public Uri FileUri { get; set; }

        public double Progress
        {
            get { return _download.Progress; }
            set
            {
                if (_download.Progress != value)
                {
                    _download.Progress = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ProgressMessage));
                    OnPropertyChanged(nameof(IsInProgress));
                }
            }
        }

        public string ProgressMessage =>
            ErrorMessage != null ? "Error" :
            (_download.Progress < 100 ? $"{_download.Progress:0.##}%" : "Done");

        public string DestinationPath => _download.DestinationPath;

        public Download Model => _download;
    }
}
