using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MessageQueue.FileMonitorService;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Topshelf;
using Topshelf.Logging;

namespace MessageQueue.ProcessingService
{
    internal class ProcessingService : ServiceControl
    {
        private readonly List<string> _fileSeqence;
        private readonly Timer _sequanceCountdown;
        private string _outputDirectory;
        private int _sequanceTime;
        private readonly CancellationTokenSource _cancelationSource;
        private string _fileMonitorQueueName;
        private string _fileQueueName;

        public ProcessingService()
        {
            _fileSeqence = new List<string>();
            _sequanceCountdown = new Timer(SequanceProcess);
            _cancelationSource = new CancellationTokenSource();
        }

        public bool Start(HostControl hostControl)
        {
            return Initialize();
        }

        private bool Initialize()
        {
            try
            {
                _fileQueueName = ConfigurationManager.AppSettings["FileQueueName"];
                _fileMonitorQueueName = ConfigurationManager.AppSettings["FileMonitorQueueName"];
                _sequanceTime = int.Parse(ConfigurationManager.AppSettings["SequanceTime"]);
                _outputDirectory = ConfigurationManager.AppSettings["OutputDirectory"];
                if (!Directory.Exists(_outputDirectory))
                {
                    Directory.CreateDirectory(_outputDirectory);
                }

                Task.Run(() => ProcessFiles(_cancelationSource.Token));
            }
            catch (Exception e)
            {
                HostLogger.Get<ProcessingService>().Error(e.Message);
                return false;
            }

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            _cancelationSource.Cancel();
            return true;
        }


        public void ProcessFiles(CancellationToken token)
        {
            if (!System.Messaging.MessageQueue.Exists(_fileQueueName))
            {
                System.Messaging.MessageQueue.Create(_fileQueueName);
            }

            using (var serverQueue = new System.Messaging.MessageQueue(_fileQueueName))
            {
                serverQueue.Formatter = new XmlMessageFormatter(new[] { typeof(FileChunk) });
                var chunks = new List<FileChunk>();

                do
                {
                    var fileParts = serverQueue.GetAllMessages();
                    foreach (var filePart in fileParts)
                    {
                        var chunk = filePart.Body as FileChunk;
                        if (chunk != null)
                        {
                            chunks.Add(chunk);
                            if (chunk.FilePosition == chunk.Size)
                            {
                                SaveFile(chunks);
                            }
                        }
                    }
                    serverQueue.Purge();
                    Thread.Sleep(1000);
                }
                while (!token.IsCancellationRequested);
            }
        }

        private void SaveFile(List<FileChunk> chunks)
        {
            var fileName = chunks.FirstOrDefault()?.FileName;
            if (fileName != null)
            {
                var resultFilePath = Path.Combine(_outputDirectory, fileName);
                using (var destination = File.Create(resultFilePath))
                {
                    foreach (var chunk in chunks)
                    {
                        destination.Write(chunk.Data, 0, chunk.Size);
                    }
                }
            }
        }

        private void SequanceProcess(object state)
        {
            try
            {
                lock (_fileSeqence)
                {
                    var cancelToken = _cancelationSource.Token;
                    if (_fileSeqence.Count == 0)
                    {
                        return;
                    }

                    HostLogger.Get<ProcessingService>().Info("Pdf generation started...");

                    using (var pdfFile = new PdfDocument())
                    {
                        foreach (var imageFile in _fileSeqence)
                        {
                            HostLogger.Get<ProcessingService>().Info($"Adding of file: {imageFile}");
                            var page = pdfFile.AddPage();
                            var gfx = XGraphics.FromPdfPage(page);
                            using (var image = XImage.FromFile(imageFile))
                            {
                                var imageWidth = (double)(image.PixelWidth < page.Width ? image.PixelWidth : page.Width);
                                var imageHeight = (imageWidth / image.PixelWidth) * image.PixelHeight;
                                gfx.DrawImage(image, 0, 0, imageWidth, imageHeight);
                            }

                            cancelToken.ThrowIfCancellationRequested();
                        }

                        HostLogger.Get<ProcessingService>().Info("Pdf generation finished...");
                        var resultPdfFile = Path.Combine(_outputDirectory,
                            $"{DateTime.Now.ToString("yyyy-MMMM-dd(HH-mm-ss)")}.pdf");
                        HostLogger.Get<ProcessingService>().Info($"Pdf file saving: \n {resultPdfFile}");
                        pdfFile.Save(resultPdfFile);
                    }
                }
            }
            catch (Exception e)
            {
                HostLogger.Get<ProcessingService>().Error(e.Message);
            }
        }

        private void ProcessFile(string sourceFilePath, string fileName)
        {
            var resultFilePath = Path.Combine(_outputDirectory, fileName);

            if (IsFileValid(resultFilePath))
            {
                lock (_fileSeqence)
                {
                    _fileSeqence.Add(resultFilePath);
                }
            }
            else
            {
                HostLogger.Get<ProcessingService>().Error($"Recieved invalid file {sourceFilePath}");
            }

            _sequanceCountdown.Change(_sequanceTime, _sequanceTime);
            HostLogger.Get<ProcessingService>().Info("Success");
        }

        private bool IsFileValid(string resultFilePath)
        {
            return Regex.IsMatch(resultFilePath, @".*[.](jpg|jpeg|png)$");
        }
    }
}
