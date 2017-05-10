using System;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Threading;
using Multithreading.Enums;
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
            download.DownloadProgressChanged += client_DownloadProgressChanged;
            download.DownloadFileCompleted += client_DownloadFileCompleted;
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
                }
            }
        }

        public bool HasError => ErrorMessage != null;

        public double Progress
        {
            get { return Model.Progress; }
            set
            {
                if (Model.Progress != value)
                {
                    Model.Progress = value;
                    Dispatcher.CurrentDispatcher.BeginInvoke((Action)delegate
                    {
                        OnPropertyChanged();
                        OnPropertyChanged(nameof(ProgressMessage));
                        OnPropertyChanged(nameof(State));
                    });
                }
            }
        }

        public string ProgressMessage =>
            ErrorMessage != null ? "Error" :
            (Model.State == DownloadState.InProgress 
                ? (Model.Progress < 100 ? $"{Model.Progress:0.##}%" : "Done")
                : Model.State.ToString());

        public string DestinationPath => Model.DestinationPath;

        public int QueueNumber => Model.Queue.Number;

        public DownloadState State => Model.State;

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (e == null)
            {
                Progress = 0;
                OnPropertyChanged(nameof(ProgressMessage));
                return;
            }

            Progress = e.ProgressPercentage;
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            OnPropertyChanged(nameof(State));
            if (Model.State == DownloadState.InQueue || Model.State == DownloadState.Cancelled)
            {
                OnPropertyChanged(nameof(ProgressMessage));
                return;
            }

            Dispose();
            if (!e.Cancelled && e.Error != null)
            {
                ErrorMessage = e.Error.InnerException?.Message ?? e.Error.Message;
                Dispatcher.CurrentDispatcher.BeginInvoke((Action)delegate
                {
                    MessageBox.Show(e.Error.InnerException?.Message ?? e.Error?.Message, "Download error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        public void Dispose()
        {
            Model.DownloadProgressChanged -= client_DownloadProgressChanged;
            Model.DownloadFileCompleted -= client_DownloadFileCompleted;
        }
    }
}
