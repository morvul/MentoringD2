using System;
using System.Net;
using Multithreading.Models;

namespace Multithreading.ViewModels
{
    public class DownloadViewModel : ViewModelBase<Download>
    {
        public DownloadViewModel()
            : base(new Download())
        {
        }

        public DownloadViewModel(Download download)
            : base(download)
        {
        }

        public string FileName
        {
            get { return Model.FileName; }
            set
            {
                if (Model.FileName != value)
                {
                    Model.FileName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ErrorMessage
        {
            get { return Model.ErrorMessage; }
            set
            {
                if (Model.ErrorMessage != value)
                {
                    Model.ErrorMessage = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ProgressMessage));
                    OnPropertyChanged(nameof(IsInProgress));
                }
            }
        }

        public bool HasError => ErrorMessage != null;

        public bool IsInProgress => Progress < 100 && !HasError;

        public WebClient WebClient => Model.WebClient;

        public Uri FileUri { get; set; }

        public double Progress
        {
            get { return Model.Progress; }
            set
            {
                if (Model.Progress != value)
                {
                    Model.Progress = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ProgressMessage));
                    OnPropertyChanged(nameof(IsInProgress));
                }
            }
        }

        public string ProgressMessage =>
            ErrorMessage != null ? "Error" :
            (Model.Progress < 100 ? $"{Model.Progress:0.##}%" : "Done");

        public string DestinationPath => Model.DestinationPath;

        public int QueueNumber => Model.Queue.Number;
    }
}
