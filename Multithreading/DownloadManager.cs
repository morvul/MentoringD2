using System;
using System.IO;
using System.Threading;
using Multithreading.Models;

namespace Multithreading
{
    public class DownloadManager
    {
        public static Download CreateDownload(string sourcePath, string destinationPath)
        {
            var download = new Download
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
                var resultFile = Path.Combine(download.DestinationPath, download.FileName);
                download.WebClient.DownloadFileAsync(download.SourcePath, resultFile,
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
    }
}
