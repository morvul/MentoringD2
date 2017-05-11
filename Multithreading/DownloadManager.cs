using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Multithreading.Models;

namespace Multithreading
{
    public class DownloadManager
    {
        private static DownloadManager _instance;

        public static DownloadManager GetInstance()
        {
            return _instance ?? (_instance = new DownloadManager());
        }

        private DownloadManager()
        {
            Downloads = new ObservableCollection<Download>();
            Queues = new ObservableCollection<Queue>();
            DefaultQueue = new Queue { Description = "Default" };
            Queues.Add(DefaultQueue);
        }

        public Queue DefaultQueue { get; set; }

        public ObservableCollection<Download> Downloads { set; get; }

        public ObservableCollection<Queue> Queues { set; get; }

        public static Download CreateDownload(string sourcePath, string destinationPath,
            Queue queue)
        {
            var download = new Download(queue)
            {
                DestinationPath = destinationPath
            };
            SetFileName(sourcePath, download);
            return download;
        }

        private static void SetFileName(string sourcePath, Download download)
        {
            string fileName = null;
            try
            {
                var fileUri = new Uri(sourcePath);
                fileName = Path.GetFileName(fileUri.LocalPath);
                download.SourcePath = fileUri;
            }
            catch (Exception expt)
            {
                download.ErrorMessage = expt.Message;
            }

            download.FileName = fileName;
        }

        public void AddDownload(Download download)
        {
            Downloads.Add(download);
            download.Queue.AddDownload(download);
        }

        public void CreateQueue(Queue newQueue)
        {
            Queues.Add(newQueue);
            UpdateQueuesNumbers();
        }

        private void UpdateQueuesNumbers()
        {
            var queueNumber = Queue.DefaultNumber;
            foreach (var queue in Queues)
            {
                queue.Number = queueNumber++;
            }
        }

        public void CancelDownload(Download download)
        {
            download.Cancel();
            Downloads.Remove(download);
            download.Queue.RemoveDownload(download);
        }

        public void AbortQueue(Queue queue)
        {
            foreach (var download in queue.Downloads.ToList())
            {
                download.Cancel();
                download.Queue.RemoveDownload(download);
                CancelDownload(download);
            }

            Queues.Remove(queue);
        }

        public void StopQueue(Queue queue)
        {
            queue.Stop();
        }
    }
}
