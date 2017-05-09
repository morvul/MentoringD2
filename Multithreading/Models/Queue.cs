using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Multithreading.Enums;

namespace Multithreading.Models
{
    public class Queue
    {
        public const int DefaultNumber = 1;

        public Queue()
        {
            Number = DefaultNumber;
            Downloads = new List<Download>();
            Type = QueueType.Sequence;
        }

        public int Number { get; set; }

        public List<Download> Downloads { get; set; }

        public string Description { get; set; }

        public string Name => Description ?? Number.ToString();

        public QueueType Type { get; set; }

        public bool IsInProgress => Downloads.Any(x => x.State == DownloadState.InProgress);

        public event Action IsInProgressChanged;
        
        public Queue Clone()
        {
            return new Queue
            {
                Type = Type,
                Number = Number,
                Downloads = Downloads,
                Description = Description
            };
        }

        public void AddDownload(Download download)
        {
            lock (Downloads)
            {
                Downloads.Add(download);
                UpdateQueueState();
            }
        }

        public void UpdateQueueState()
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
                        var downloadsInRun = Downloads
                            .Where(x => x.State == DownloadState.InProgress)
                            .ToList();
                        if (downloadsInRun.Count != 1)
                        {
                            foreach (var download in downloadsInRun)
                            {
                                download.Suspend();
                            }

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
                var currentDownload = Downloads.FirstOrDefault();
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
