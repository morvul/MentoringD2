using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using Topshelf;
using Topshelf.Logging;

namespace Services
{
    internal class DocumentControlSystemService : ServiceControl
    {
        private readonly List<FileSystemWatcher> _fileWatchers;
        private readonly List<string> _fileSeqence;
        private readonly Timer _sequanceCountdown;
        private string _outputDirectory;
        private string _trashDirectory;
        private int _sequanceTime;
        private string _processedDirectory;

        public DocumentControlSystemService()
        {
            _fileWatchers = new List<FileSystemWatcher>();
            _fileSeqence = new List<string>();
            _sequanceCountdown = new Timer(SequanceProcess);
        }

        public bool Start(HostControl hostControl)
        {
            return Initialize();
        }

        private bool Initialize()
        {
            try
            {
                _sequanceTime = int.Parse(ConfigurationManager.AppSettings["SequanceTime"]);
                _outputDirectory = ConfigurationManager.AppSettings["OutputDirectory"];
                _trashDirectory = ConfigurationManager.AppSettings["TrashDirectory"];
                _processedDirectory = ConfigurationManager.AppSettings["ProcessedDirectory"];
                var targetDirectories = ConfigurationManager
                    .AppSettings["TargetDirectories"]
                    .Split(';')
                    .Select(x => x.Trim())
                    .ToList();
                if (!Directory.Exists(_outputDirectory))
                {
                    Directory.CreateDirectory(_outputDirectory);
                }

                if (!Directory.Exists(_trashDirectory))
                {
                    Directory.CreateDirectory(_trashDirectory);
                }

                if (!Directory.Exists(_processedDirectory))
                {
                    Directory.CreateDirectory(_processedDirectory);
                }

                ProcessFolder(_outputDirectory);
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

                _fileWatchers.Clear();
            }
        }
        private void SequanceProcess(object state)
        {
            try
            {
                lock (_fileSeqence)
                {
                    if (_fileSeqence.Count == 0)
                    {
                        return;
                    }

                    HostLogger.Get<DocumentControlSystemService>().Info("Pdf generation started...");

                    var pdfFile = new Document();
                    var section = pdfFile.AddSection();
                    foreach (var file in _fileSeqence.ToList())
                    {
                        var fileName = Path.GetFileName(file);
                        if (fileName != null)
                        {
                            HostLogger.Get<DocumentControlSystemService>().Info($"Adding of file: {file}");
                            var image = section.AddImage(file);
                            image.Height = pdfFile.DefaultPageSetup.PageHeight;
                            image.Width = pdfFile.DefaultPageSetup.PageWidth;
                            image.ScaleHeight = 0.75;
                            image.ScaleWidth = 0.75;

                            section.AddPageBreak();
                            var pocessedFilePath = Path.Combine(_processedDirectory, fileName);
                            MoveWithRenaming(file, pocessedFilePath);
                            _fileSeqence.Remove(file);
                        }
                    }

                    var render = new PdfDocumentRenderer {Document = pdfFile};
                    render.RenderDocument();
                    HostLogger.Get<DocumentControlSystemService>().Info("Pdf generation finished...");
                    var resultPdfFile = Path.Combine(_processedDirectory, $"{DateTime.Now.ToString("yyyy-MMMM-dd(HH-mm-ss)")}.pdf");
                    HostLogger.Get<DocumentControlSystemService>().Info($"Pdf file saving: \n {resultPdfFile}");

                    render.Save(resultPdfFile);
                }
            }
            catch (Exception e)
            {
                HostLogger.Get<DocumentControlSystemService>().Error(e.Message);
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
                        fileWatcher.Changed -= NewFile;
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
            if (sourceFilePath != resultFilePath)
            {
                MoveWithRenaming(sourceFilePath, resultFilePath);
            }

            if (IsFileValid(resultFilePath))
            {
                lock (_fileSeqence)
                {
                    _fileSeqence.Add(resultFilePath);
                }
            }
            else
            {
                var trashFilePath = Path.Combine(_trashDirectory, fileName);
                MoveWithRenaming(resultFilePath, trashFilePath);
            }

            _sequanceCountdown.Change(0, _sequanceTime);
            HostLogger.Get<DocumentControlSystemService>().Info("Success");
        }

        private void MoveWithRenaming(string sourceFilePath, string resultFilePath)
        {
            if (File.Exists(resultFilePath))
            {
                var dir = Path.GetDirectoryName(resultFilePath);
                var file = Path.GetFileNameWithoutExtension(resultFilePath);
                var ext = Path.GetExtension(resultFilePath);
                resultFilePath = Path.Combine(dir, file + " - Copy" + ext);
            }

            HostLogger.Get<DocumentControlSystemService>().Info($"Copying of file started:\n {sourceFilePath}\n ->\n {resultFilePath}");

            File.Move(sourceFilePath, resultFilePath);
        }

        private bool IsFileValid(string resultFilePath)
        {
            return Regex.IsMatch(resultFilePath, @".*[.](jpg|jpeg|png)$");
        }
    }
}
