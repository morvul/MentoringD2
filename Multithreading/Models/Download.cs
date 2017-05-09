using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using Multithreading.Enums;

namespace Multithreading.Models
{
    public class Download
    {
        private readonly WebClient _webClient;

        public Download()
            : this(new Queue())
        {
        }

        public Download(Queue queue)
        {
            _webClient = new WebClient();
            _webClient.DownloadProgressChanged += (sender, args) =>
                DownloadProgressChanged?.Invoke(sender, args);
            _webClient.DownloadFileCompleted += DownloadCompleted;
            Queue = queue;
            State = DownloadState.InQueue;
        }

        public string ErrorMessage { get; set; }

        public double Progress { get; set; }

        public bool HasError => ErrorMessage != null;

        public Uri SourcePath { get; set; }

        public string DestinationPath { get; set; }

        public string FileName { get; set; }

        public string DestinationalFile => Path.Combine(DestinationPath, FileName);

        public Queue Queue { get; set; }

        public DownloadState State { get; set; }

        public event EventHandler<AsyncCompletedEventArgs> DownloadFileCompleted;

        public event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;

        public void Start()
        {

            if (State == DownloadState.Cancelled)
            {
                return;
            }

            if (_webClient.IsBusy)
            {
                State = DownloadState.Restart;
            }

            if (State == DownloadState.Restart)
            {
                return;
            }

            State = DownloadState.InProgress;
            _webClient.DownloadFileAsync(SourcePath, DestinationalFile);
            DownloadProgressChanged?.Invoke(this, null);
        }

        public void Cancel()
        {
            State = DownloadState.Cancelled;
            if (_webClient.IsBusy)
            {
                _webClient.CancelAsync();
            }
            else
            {
                var args = new AsyncCompletedEventArgs(null, true, null);
                DownloadFileCompleted?.Invoke(this, args);
                _webClient.DownloadFileCompleted -= DownloadCompleted;
            }
        }

        public void Suspend()
        {
            State = DownloadState.InQueue;
            _webClient.CancelAsync();
        }

        private void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (State == DownloadState.Restart)
            {
                State = DownloadState.InQueue;
                Start();
                return;
            }

            DownloadFileCompleted?.Invoke(this, e);
            if (State == DownloadState.InQueue)
            {
                return;
            }

            _webClient.DownloadFileCompleted -= DownloadCompleted;
            if (State == DownloadState.Cancelled)
            {
                try
                {
                    File.Delete(DestinationalFile);
                }
                catch (Exception fileError)
                {
                    ErrorMessage = fileError.Message;
                    State = DownloadState.HasError;
                    return;
                }

                return;
            }

            State = e.Error != null ? DownloadState.HasError : DownloadState.Completed;
        }
    }
}
