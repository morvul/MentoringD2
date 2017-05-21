using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Text.RegularExpressions;
using Topshelf;
using Topshelf.Logging;

namespace MessageQueue.FileMonitorService
{
    internal class DocumentControlSystemService : ServiceControl
    {
        private readonly List<FileSystemWatcher> _fileWatchers;
        private string _trashDirectory;
        private string _processedDirectory;
        private string _fileQueueName;

        public DocumentControlSystemService()
        {
            _fileWatchers = new List<FileSystemWatcher>();
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
            var files = new List<FileInfo>();
            foreach (var filePath in fileSeqence)
            {
                var file = new FileInfo(filePath);
                files.Add(file);
            }

            files = files.OrderBy(x => x.CreationTime).ToList();
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
            var resultFilePath = Path.Combine(_processedDirectory, fileName);
            if (IsFileValid(resultFilePath))
            {
                resultFilePath = MoveWithRenaming(sourceFilePath, resultFilePath);
                TranserFile(resultFilePath);
            }
            else
            {
                var trashFilePath = Path.Combine(_trashDirectory, fileName);
                MoveWithRenaming(resultFilePath, trashFilePath);
            }
        }

        private string MoveWithRenaming(string sourceFilePath, string resultFilePath)
        {
            var dir = Path.GetDirectoryName(resultFilePath);
            var fileName = Path.GetFileNameWithoutExtension(resultFilePath);
            var ext = Path.GetExtension(resultFilePath);
            while (File.Exists(resultFilePath))
            {
                fileName = fileName + " - Copy";
                resultFilePath = Path.Combine(dir, fileName + ext);
            }

            HostLogger.Get<DocumentControlSystemService>().Info($"Moving of file started:\n {sourceFilePath}\n ->\n {resultFilePath}");

            File.Move(sourceFilePath, resultFilePath);
            return resultFilePath;
        }

        private bool IsFileValid(string resultFilePath)
        {
            return Regex.IsMatch(resultFilePath, @".*[.](jpg|jpeg|png)$");
        }

        private void TranserFile(string resultFilePath)
        {
            HostLogger.Get<DocumentControlSystemService>().Info($"File Sending started:\n{resultFilePath}");

            try
            {
                if (!System.Messaging.MessageQueue.Exists(_fileQueueName))
                {
                    System.Messaging.MessageQueue.Create(_fileQueueName);
                }

                using (FileStream file = new FileStream(resultFilePath, FileMode.Open))
                {
                    var buffer = new byte[1024*1024*4];
                    int bytesRead;
                    while ((bytesRead = file.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        var fileChunk = new FileChunk()
                        {
                            FilePosition = file.Position,
                            FileSize = file.Length,
                            Data = buffer,
                            Size = bytesRead
                        };

                        using (var serverQueue = new System.Messaging.MessageQueue(_fileQueueName, QueueAccessMode.Send))
                        {
                            var message = new Message(fileChunk);
                            serverQueue.Send(message);
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
