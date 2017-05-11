using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Multithreading.Enums;

namespace Multithreading.Models
{
    public class Queue
    {
        public const int DefaultNumber = 1;
        private readonly int _scheduleCheckDelay;
        private Task _queueScheduler;

        public Queue()
        {
            Number = DefaultNumber;
            Downloads = new List<Download>();
            Type = QueueType.Sequence;
            _queueScheduler = new Task(QueueScheduler);
            var delayStr = ConfigurationManager.AppSettings["ScheduleCheckDelay"];
            _scheduleCheckDelay = int.Parse(delayStr);
            StartTime = DateTime.Now;
        }

        private void QueueScheduler()
        {
            lock (Downloads)
            {
                Downloads
                    .Where(x => x.State == DownloadState.InQueue)
                    .ToList()
                    .ForEach(x => x.State = DownloadState.Scheduled);
            }

            while (StartTime > DateTime.Now)
            {
                Thread.Sleep(_scheduleCheckDelay);
            }

            lock (Downloads)
            {
                Downloads
                    .Where(x => x.State == DownloadState.Scheduled)
                    .ToList()
                    .ForEach(x => x.State = DownloadState.InQueue);
            }

            Application.Current.Dispatcher.BeginInvoke((Action)UpdateQueueState);
        }

        public int Number { get; set; }

        public List<Download> Downloads { get; set; }

        public string Description { get; set; }

        public string Name => Description ?? Number.ToString();

        public QueueType Type { get; set; }

        public bool IsInProgress => Downloads.Any(x => x.State == DownloadState.InProgress);

        public DateTime StartTime { get; set; }

        public event Action IsInProgressChanged;
        
        public Queue Clone()
        {
            return new Queue
            {
                Type = Type,
                Number = Number,
                Downloads = Downloads,
                Description = Description,
                StartTime = StartTime
            };
        }

        public void AddDownload(Download download)
        {
            lock (Downloads)
            {
                Downloads.Add(download);
                Refresh();
            }
        }

        public void Refresh()
        {
            if (_queueScheduler.Status != TaskStatus.Running)
            {
                _queueScheduler = Task.Run(()=>QueueScheduler());
            }
        }

        private void UpdateQueueState()
        {
            lock (Downloads)
            {
                switch (Type)
                {
                    case QueueType.Parallel:
                        var downloadsForStartup = Downloads.Where(x => x.State == DownloadState.InQueue);
                        foreach (var download in downloadsForStartup)
                        {
                            download.Start();
                        }

                        break;
                    case QueueType.Sequence:
                        var firstDownload = Downloads.FirstOrDefault(x => x.State == DownloadState.InProgress);
                        var downloadsInRun = Downloads
                            .Where(x => x.State == DownloadState.InProgress)
                            .ToList();
                            foreach (var download in downloadsInRun)
                            {
                                if (download != firstDownload)
                                {
                                    download.Suspend();
                                }
                            }

                        if (firstDownload == null)
                        {
                            TakeSequanceItem();
                        }

                        break;
                }
            }
        }

        private void TakeSequanceItem()
        {
            lock (Downloads)
            {
                var currentDownload = Downloads.FirstOrDefault(x => x.State == DownloadState.InQueue);
                if (currentDownload != null)
                {
                    currentDownload.Start();
                    currentDownload.DownloadFileCompleted += SequanceDownloadCompleted;
                }

                IsInProgressChanged?.Invoke();
            }
        }

        private void SequanceDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            var download = sender as Download;
            NextSequenceItem(download);
        }

        private void NextSequenceItem(Download download)
        {
            lock (Downloads)
            {
                if (download != null)
                {
                    download.DownloadFileCompleted -= SequanceDownloadCompleted;
                    Downloads.Remove(download);
                }
            }

            TakeSequanceItem();
        }

        public void RemoveDownload(Download download)
        {
            lock (Downloads)
            {
                Downloads.Remove(download);
            }
        }

        public void Stop()
        {
            lock (Downloads)
            {
                foreach (var download in Downloads)
                {
                    download.Cancel();
                }
            }
        }
    }
}
