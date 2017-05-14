using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Topshelf;
using Topshelf.Logging;

namespace Services
{
    internal class DocumentControlSystemService : ServiceControl
    {
        private readonly List<FileSystemWatcher> _fileWatchers;
        private string _outputDirectory;

        public DocumentControlSystemService()
        {
            _fileWatchers = new List<FileSystemWatcher>();
        }

        public bool Start(HostControl hostControl)
        {
            return Initialize();
        }

        private bool Initialize()
        {
            try
            {
                _outputDirectory = ConfigurationManager.AppSettings["OutputDirectory"];
                var targetDirectories = ConfigurationManager
                    .AppSettings["TargetDirectories"]
                    .Split(';')
                    .Select(x => x.Trim())
                    .ToList();
                if (!Directory.Exists(_outputDirectory))
                {
                    Directory.CreateDirectory(_outputDirectory);
                }

                foreach (var targetDirectory in targetDirectories)
                {
                    ProcessFolder(targetDirectory);
                }

                InitializeWatchers(targetDirectories);
            }
            catch (Exception e)
            {
                HostLogger.Get<DocumentControlSystemService>().Error(e.Message);
                return false;
            }

            return true;
        }


        public bool Stop(HostControl hostControl)
        {
            DisposeWatchers();
            return true;
        }

        private void ProcessRun()
        {
            HostLogger.Get<DocumentControlSystemService>().Info("");
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
                    fileWatcher.EnableRaisingEvents = true;
                    _fileWatchers.Add(fileWatcher);
                }

                _fileWatchers.Clear();
            }
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
                        fileWatcher.Dispose();
                    }
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
            foreach (var sourceFilePath in Directory.GetFiles(targetDirectory))
            {
                var fileName = Path.GetFileName(sourceFilePath);
                if (fileName != null)
                {
                    ProcessFile(sourceFilePath, fileName);
                }
            }
        }

        private void ProcessFile(string sourceFilePath, string fileName)
        {
            var resultFilePath = Path.Combine(_outputDirectory, fileName);
            HostLogger.Get<DocumentControlSystemService>().Info($"Copying of file started:\n {sourceFilePath}\n ->\n {resultFilePath}");
            File.Move(sourceFilePath, resultFilePath);
            HostLogger.Get<DocumentControlSystemService>().Info("Success");
        }
    }
}
