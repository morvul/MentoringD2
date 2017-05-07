using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using Multithreading.Models;
using Multithreading.ViewModels;

namespace Multithreading
{
    public static class DownloadManager
    {
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

        public static void StartDownload(Download download, object listener = null)
        {
            var thread = new Thread(() =>
            {
                download.WebClient.DownloadFileAsync(download.SourcePath, download.DestinationalFile,
                    listener);
            });
            thread.Start();
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

        public static void AbortDownload(Download download)
        {
            download.WebClient.CancelAsync();
            download.WebClient.DownloadFileCompleted += (sender, e) =>
                File.Delete(download.DestinationalFile);
        }

        public static void AbortQueue(Queue model)
        {
            throw new NotImplementedException();
        }

        public static void UpdateQueuesNumbers(ObservableCollection<QueueViewModel> queues)
        {
            var queueNumber = Queue.DefaultNumber;
            foreach (var queue in queues)
            {
                queue.Number = queueNumber++;
            }
        }
    }
}
