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
        private readonly Dictionary<Guid, Sequance> _sequances;
        private string _outputDirectory;
        private int _sequanceTime;
        private readonly CancellationTokenSource _cancelationSource;
        private string _fileQueueName;
        private string _trashDirectory;

        public ProcessingService()
        {
            _sequances = new Dictionary<Guid, Sequance>();
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
                _sequanceTime = int.Parse(ConfigurationManager.AppSettings["SequanceTime"]);
                _outputDirectory = ConfigurationManager.AppSettings["OutputDirectory"];
                _trashDirectory = ConfigurationManager.AppSettings["TrashDirectory"];
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


        private void ProcessFiles(CancellationToken token)
        {
            try
            {
                using (var serverQueue = new System.Messaging.MessageQueue(_fileQueueName, QueueAccessMode.Receive))
                {
                    serverQueue.Formatter = new XmlMessageFormatter(new[] {typeof (FileChunk)});
                    var chunks = new Dictionary<Guid, List<FileChunk>>();

                    do
                    {
                        var message = serverQueue.Receive();
                        var chunk = message?.Body as FileChunk;
                        if (chunk != null)
                        {
                            if (!chunks.ContainsKey(chunk.AgentId))
                            {
                                chunks.Add(chunk.AgentId, new List<FileChunk>());
                            }

                            chunks[chunk.AgentId].Add(chunk);
                            if (chunk.FilePosition == chunk.FileSize)
                            {
                                var fileName = SaveFile(chunks[chunk.AgentId]);
                                ProcessFile(fileName, chunk.AgentId, token);
                                chunks[chunk.AgentId].Clear();
                            }
                        }
                    } while (!token.IsCancellationRequested);
                }
            }
            catch (Exception e)
            {
                HostLogger.Get<ProcessingService>().Error(e.Message);
            }
        }

        private string SaveFile(List<FileChunk> chunks)
        {
            var fileName = chunks.FirstOrDefault()?.FileName;
            if (fileName != null)
            {
                var resultFilePath = FileHelper.GetUniqueName(_outputDirectory, fileName);
                using (var destination = File.Create(resultFilePath))
                {
                    foreach (var chunk in chunks)
                    {
                        destination.Write(chunk.Data, 0, chunk.Size);
                    }
                }

                return resultFilePath;
            }

            return null;
        }

        private void SequanceProcess(List<string> fileSeqence,  Guid agentId)
        {
            try
            {
                lock (fileSeqence)
                {
                    var cancelToken = _cancelationSource.Token;
                    if (fileSeqence.Count == 0)
                    {
                        return;
                    }

                    HostLogger.Get<ProcessingService>().Info("Pdf generation started...");

                    using (var pdfFile = new PdfDocument())
                    {
                        foreach (var imageFile in fileSeqence)
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
                        var agentName = agentId.ToString().Substring(15, 4);
                        var resultPdfFile = Path.Combine(_outputDirectory,
                            $"{DateTime.Now:yyyy-MMMM-dd(HH-mm-ss)} - agent {agentName}.pdf");
                        HostLogger.Get<ProcessingService>().Info($"Pdf file saving: \n {resultPdfFile}");
                        pdfFile.Save(resultPdfFile);
                    }

                    foreach (var file in fileSeqence)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception e)
            {
                HostLogger.Get<ProcessingService>().Error(e.Message);
            }
        }

        private void ProcessFile(string filePath, Guid agentId, CancellationToken token)
        {
            if (IsFileValid(filePath))
            {
                lock (_sequances)
                {
                    if (!_sequances.ContainsKey(agentId))
                    {
                        var newSequance = new Sequance(agentId, _sequanceTime, token);
                        newSequance.OnSequanceCompleted += SequanceProcess;
                        _sequances.Add(agentId, newSequance);
                    }

                    _sequances[agentId].AddSequanceItem(filePath);
                }
            }
            else
            {
                var fileName = Path.GetFileName(filePath);
                var agentName = agentId.ToString().Substring(0, 4);
                var trashFilePath = Path.Combine(_trashDirectory, agentName, fileName);
                FileHelper.MoveWithRenaming(filePath, trashFilePath);
                HostLogger.Get<ProcessingService>().Error($"Recieved invalid file {filePath}");
            }
        }

        private bool IsFileValid(string resultFilePath)
        {
            return Regex.IsMatch(resultFilePath, @".*[.](jpg|jpeg|png)$");
        }
    }
}
