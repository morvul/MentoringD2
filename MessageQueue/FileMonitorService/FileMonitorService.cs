using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Messaging;
using Topshelf;
using Topshelf.Logging;

namespace MessageQueue.FileMonitorService
{
    internal class DocumentControlSystemService : ServiceControl
    {
        private readonly List<FileSystemWatcher> _fileWatchers;
        private readonly Guid _instanceId;
        private string _remoteControlQueueName;
        private string _trashDirectory;
        private string _processedDirectory;
        private string _fileQueueName;

        public DocumentControlSystemService()
        {
            _fileWatchers = new List<FileSystemWatcher>();
            _instanceId = Guid.NewGuid();
        }

        public bool Start(HostControl hostControl)
        {
            return Initialize();
        }

        public bool Stop(HostControl hostControl)
        {
            DisposeWatchers();
            return true;
        }

        private bool Initialize()
        {
            try
            {
                _remoteControlQueueName = ConfigurationManager.AppSettings["RemoteControlQueueName"];
                _fileQueueName = ConfigurationManager.AppSettings["FileQueueName"];
                _trashDirectory = ConfigurationManager.AppSettings["TrashDirectory"];
                _processedDirectory = ConfigurationManager.AppSettings["ProcessedDirectory"];
                var targetDirectories = ConfigurationManager
                    .AppSettings["TargetDirectories"]
                    .Split(';')
                    .Select(x => x.Trim())
                    .ToList();
                InitializeFolders(targetDirectories);
                ProcessFolders(targetDirectories);
                InitializeWatchers(targetDirectories);
            }
            catch (Exception e)
            {
                HostLogger.Get<DocumentControlSystemService>().Error(e.Message);
                return false;
            }

            return true;
        }

        private void ProcessFolders(List<string> targetDirectories)
        {
            foreach (var targetDirectory in targetDirectories)
            {
                ProcessFolder(targetDirectory);
            }
        }

        private void InitializeFolders(List<string> targetDirectories)
        {
            if (!Directory.Exists(_trashDirectory))
            {
                Directory.CreateDirectory(_trashDirectory);
            }

            if (!Directory.Exists(_processedDirectory))
            {
                Directory.CreateDirectory(_processedDirectory);
            }

            foreach (var targetDirectory in targetDirectories)
            {
                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }
            }
        }

        private void InitializeWatchers(List<string> targetDirectories)
        {
            lock (_fileWatchers)
            {
                foreach (var targetDirectory in targetDirectories)
                {
                    if (!Directory.Exists(targetDirectory))
                    {
                        Directory.CreateDirectory(targetDirectory);
                    }

                    var fileWatcher = new FileSystemWatcher(targetDirectory);
                    fileWatcher.Created += NewFile;
                    fileWatcher.Renamed += NewFile;
                    fileWatcher.Changed += NewFile;
                    fileWatcher.EnableRaisingEvents = true;
                    _fileWatchers.Add(fileWatcher);
                }
            }
        }
        private List<FileInfo> GetFiles(List<string> fileSeqence)
        {
            var files = fileSeqence
                .Select(filePath => new FileInfo(filePath))
                .OrderBy(x => x.CreationTime)
                .ToList();
            return files;
        }

        private void DisposeWatchers()
        {
            try
            {
                lock (_fileWatchers)
                {
                    foreach (var fileWatcher in _fileWatchers)
                    {
                        fileWatcher.EnableRaisingEvents = false;
                        fileWatcher.Created -= NewFile;
                        fileWatcher.Renamed -= NewFile;
                        fileWatcher.Changed -= NewFile;
                        fileWatcher.Dispose();
                    }

                    _fileWatchers.Clear();
                }
            }
            catch (Exception e)
            {
                HostLogger.Get<DocumentControlSystemService>().Error(e.Message);
            }
        }

        private void NewFile(object sender, FileSystemEventArgs e)
        {
            try
            {
                ProcessFile(e.FullPath, e.Name);
            }
            catch (Exception error)
            {
                HostLogger.Get<DocumentControlSystemService>().Error(error.Message);
            }
        }

        private void ProcessFolder(string targetDirectory)
        {
            var filenames = Directory.GetFiles(targetDirectory).ToList();
            var orderedImageFiles = GetFiles(filenames);
            foreach (var sourceFile in orderedImageFiles)
            {
                var fileName = Path.GetFileName(sourceFile.ToString());
                ProcessFile(sourceFile.ToString(), fileName);
            }
        }

        private void ProcessFile(string sourceFilePath, string fileName)
        {
            if (FileHelper.IsFileValid(sourceFilePath))
            {
                TranserFile(sourceFilePath);
                File.Delete(sourceFilePath);
            }
            else
            {
                var trashFilePath = Path.Combine(_trashDirectory, fileName);
                FileHelper.MoveWithRenaming(sourceFilePath, trashFilePath);
            }
        }

        private void TranserFile(string filePath)
        {
            HostLogger.Get<DocumentControlSystemService>().Info($"File Sending started:\n{filePath}");
            if (!System.Messaging.MessageQueue.Exists(_fileQueueName))
            {
                System.Messaging.MessageQueue.Create(_fileQueueName);
            }

            try
            {
                var fileName = Path.GetFileName(filePath);
                using (FileStream file = new FileStream(filePath, FileMode.Open))
                {
                    var buffer = new byte[1024*1024];
                    int bytesRead;
                    while ((bytesRead = file.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        var fileChunk = new FileChunk()
                        {
                            FilePosition = file.Position,
                            FileSize = file.Length,
                            Data = buffer,
                            Size = bytesRead,
                            FileName = fileName,
                            AgentId = _instanceId
                        };

                        using (var fileQueue = new System.Messaging.MessageQueue(_fileQueueName, QueueAccessMode.Send))
                        {
                            var message = new Message(fileChunk);
                            fileQueue.Send(message);
                        }
                    }
                }

                HostLogger.Get<DocumentControlSystemService>().Info("File Sending done");
            }
            catch (Exception e)
            {
                HostLogger.Get<DocumentControlSystemService>().Error(e.Message);
            }
        }
    }
}
