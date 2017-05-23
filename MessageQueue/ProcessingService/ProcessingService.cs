using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Threading;
using System.Threading.Tasks;
using MessageQueue.FileMonitorService;
using MessageQueue.ProcessingService.Enums;
using MessageQueue.RemoteController;
using MessageQueue.RemoteController.Models;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Topshelf;
using Topshelf.Logging;

namespace MessageQueue.ProcessingService
{
    internal class ProcessingService : ServiceControl
    {
        private readonly Dictionary<Guid, Sequance> _sequances;
        private readonly CancellationTokenSource _cancelationSource;
        private readonly DcsRemoteControl _remoteControl;
        private string _outputDirectory;
        private int _sequanceTime;
        private string _fileQueueName;
        private string _trashDirectory;
        private string _queuesQueueName;
        private ProcessingStatus _status;
        private int _processedCount;
        private int _pdfCount;

        public ProcessingService()
        {
            _pdfCount = 0;
            _processedCount = 0;
            _sequances = new Dictionary<Guid, Sequance>();
            _cancelationSource = new CancellationTokenSource();
            var remoteControlQueueName = ConfigurationManager.AppSettings["RemoteControlQueueName"];
            var remoteControlRecallDelay = int.Parse(ConfigurationManager.AppSettings["RemoteControlRecallDelay"]);
            _remoteControl = new DcsRemoteControl(remoteControlQueueName, remoteControlRecallDelay);
            _remoteControl.OnProcessingServiceSettingsChanged += ProcessingServiceSettingsChanged;
        }

        public bool Start(HostControl hostControl)
        {
            _status = ProcessingStatus.JustStarted;
            _remoteControl.StartProcessingServicesettingsMonitor(GetInfo);
            return Initialize();
        }

        private bool Initialize()
        {
            try
            {
                _status = ProcessingStatus.Initialization;
                _fileQueueName = ConfigurationManager.AppSettings["FileQueueName"];
                _sequanceTime = int.Parse(ConfigurationManager.AppSettings["SequanceTime"]);
                _outputDirectory = ConfigurationManager.AppSettings["OutputDirectory"];
                _trashDirectory = ConfigurationManager.AppSettings["TrashDirectory"];
                _queuesQueueName = ConfigurationManager.AppSettings["QueuesQueueName"];
                if (!Directory.Exists(_outputDirectory))
                {
                    Directory.CreateDirectory(_outputDirectory);
                }

                RestorePreviousSession();
                _status = ProcessingStatus.Idle;
                Task.Run(() => ProcessFiles(_cancelationSource.Token));
            }
            catch (Exception e)
            {
                HostLogger.Get<ProcessingService>().Error(e.Message);
                return false;
            }

            return true;
        }

        private void RestorePreviousSession()
        {
            if (!System.Messaging.MessageQueue.Exists(_queuesQueueName))
            {
                System.Messaging.MessageQueue.Create(_queuesQueueName);
            }

            using (var queuesQueue = new System.Messaging.MessageQueue(_queuesQueueName, QueueAccessMode.Receive))
            {
                queuesQueue.Formatter = new XmlMessageFormatter(new[] { typeof(AgentQueue) });
                var agentQueues = queuesQueue.GetAllMessages().Select(x => x.Body as AgentQueue);
                foreach (var agentQqueue in agentQueues)
                {
                    if (agentQqueue == null)
                    {
                        continue;
                    }

                    lock (_sequances)
                    {
                        if (!_sequances.ContainsKey(agentQqueue.AgentId))
                        {
                            var newSequance = new Sequance(agentQqueue.AgentId, _sequanceTime, _cancelationSource.Token);
                            newSequance.OnSequanceCompleted += SequanceProcess;
                            _sequances.Add(agentQqueue.AgentId, newSequance);
                            if (!System.Messaging.MessageQueue.Exists(agentQqueue.QueueName))
                            {
                                System.Messaging.MessageQueue.Create(agentQqueue.QueueName);
                            }
                        }

                        _sequances[agentQqueue.AgentId].UpdateSequanceState();
                    }
                }
            }
        }

        public bool Stop(HostControl hostControl)
        {
            _status = ProcessingStatus.Stopped;
            _remoteControl.StopProcessingServicesettingsMonitor();
            _cancelationSource.Cancel();
            return true;
        }


        private void ProcessFiles(CancellationToken token)
        {
            try
            {
                using (var fileQueue = new System.Messaging.MessageQueue(_fileQueueName, QueueAccessMode.Receive))
                {
                    fileQueue.Formatter = new XmlMessageFormatter(new[] { typeof(FileChunk) });
                    var chunks = new Dictionary<Guid, List<FileChunk>>();

                    do
                    {
                        var message = fileQueue.Receive();
                        var chunk = message?.Body as FileChunk;
                        if (chunk != null)
                        {
                            _status = ProcessingStatus.FileRecieving;
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

            _status = ProcessingStatus.Idle;
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

        private void SequanceProcess(Guid agentId, CancellationToken cancelToken)
        {
            try
            {
                var agentQueueName = GetAgentQueueName(agentId);
                HostLogger.Get<ProcessingService>().Info("Pdf generation started...");
                using (var agentQueue = new System.Messaging.MessageQueue(agentQueueName, QueueAccessMode.Receive))
                {
                    agentQueue.Formatter = new XmlMessageFormatter(new[] { typeof(string) });
                    var imageNamesMessages = agentQueue.GetAllMessages();
                    if (!imageNamesMessages.Any())
                    {
                        return;
                    }

                    var imageNames = imageNamesMessages.Select(x => x.Body as string).ToList();
                    GeneratePdfFile(agentId, imageNames, cancelToken);
                    RemoveProcessedItems(imageNames, agentQueue);
                }
            }
            catch (Exception e)
            {
                HostLogger.Get<ProcessingService>().Error(e.Message);
            }

            _status = ProcessingStatus.Idle;
        }

        private void RemoveProcessedItems(List<string> imageNames, System.Messaging.MessageQueue agentQueue)
        {
            foreach (var imageName in imageNames)
            {
                agentQueue.Receive();
                File.Delete(imageName);
            }
        }

        private void GeneratePdfFile(Guid agentId, List<string> imageNames, CancellationToken cancelToken)
        {
            _status = ProcessingStatus.PdfFileGeneration;
            using (var pdfFile = new PdfDocument())
            {
                foreach (var imageName in imageNames)
                {
                    HostLogger.Get<ProcessingService>().Info($"Adding of file: {imageName}");
                    var page = pdfFile.AddPage();
                    var gfx = XGraphics.FromPdfPage(page);
                    using (var image = XImage.FromFile(imageName))
                    {
                        var imageWidth = (double)(image.PixelWidth < page.Width ? image.PixelWidth : page.Width);
                        var imageHeight = (imageWidth / image.PixelWidth) * image.PixelHeight;
                        gfx.DrawImage(image, 0, 0, imageWidth, imageHeight);
                    }

                    cancelToken.ThrowIfCancellationRequested();
                }

                HostLogger.Get<ProcessingService>().Info("Pdf generation finished...");
                var agentName = agentId.ToString().Substring(32, 4);
                var resultPdfFile = Path.Combine(_outputDirectory,
                    $"{DateTime.Now:yyyy-MMMM-dd(HH-mm-ss)} - agent {agentName}.pdf");
                HostLogger.Get<ProcessingService>().Info($"Pdf file saving: \n {resultPdfFile}");
                pdfFile.Save(resultPdfFile);
                _pdfCount++;
            }

            _status = ProcessingStatus.Idle;
        }

        private void ProcessFile(string filePath, Guid agentId, CancellationToken token)
        {
            if (FileHelper.IsFileValid(filePath))
            {
                lock (_sequances)
                {
                    var agentQueueName = GetAgentQueueName(agentId);
                    if (!_sequances.ContainsKey(agentId))
                    {
                        _status = ProcessingStatus.FileProcessing;
                        var newSequance = new Sequance(agentId, _sequanceTime, token);
                        newSequance.OnSequanceCompleted += SequanceProcess;
                        _sequances.Add(agentId, newSequance);
                        if (!System.Messaging.MessageQueue.Exists(agentQueueName))
                        {
                            System.Messaging.MessageQueue.Create(agentQueueName);
                            using (var queuesQueue =
                                new System.Messaging.MessageQueue(_queuesQueueName, QueueAccessMode.Send))
                            {
                                var agentQueue = new AgentQueue
                                {
                                    AgentId = agentId,
                                    QueueName = agentQueueName
                                };
                                queuesQueue.Send(new Message(agentQueue));
                            }
                        }
                    }

                    _sequances[agentId].UpdateSequanceState();
                    using (var fileQueue = new System.Messaging.MessageQueue(agentQueueName, QueueAccessMode.Send))
                    {
                        var message = new Message(filePath);
                        fileQueue.Send(message);
                        _processedCount++;
                    }
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

            _status = ProcessingStatus.Idle;
        }

        private string GetAgentQueueName(Guid agentId)
        {
            return $@".\private$\AgentQueue{agentId}";
        }

        private void ProcessingServiceSettingsChanged(ProcessingServiceSettings setings)
        {
            _outputDirectory = setings.OutputDirectory;
            _trashDirectory = setings.TrashDirectory;
            _sequanceTime = setings.SequanceTime;
            lock (_sequances)
            {
                foreach (var sequance in _sequances.Values)
                {
                    sequance.UpdateSequanceSettings(_sequanceTime);
                }
            }

            HostLogger.Get<ProcessingService>().Info("Settings updated...");
        }


        private ProcessingServiceData GetInfo()
        {
            var settings = new ProcessingServiceSettings
            {
                OutputDirectory = _outputDirectory,
                TrashDirectory = _trashDirectory,
                SequanceTime = _sequanceTime
            };
            var info = new ProcessingServiceData
            {
                Settings = settings,
                Status = $"{_status} (images processed: {_processedCount}, pdf files generated: {_pdfCount})"
            };
            return info;
        }
    }
}
