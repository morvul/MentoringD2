using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
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
        private CancellationTokenSource _cancelationSource;

        public DocumentControlSystemService()
        {
            _fileWatchers = new List<FileSystemWatcher>();
            _fileSeqence = new List<string>();
            _sequanceCountdown = new Timer(SequanceProcess);
        }

        public bool Start(HostControl hostControl)
        {
            _cancelationSource = new CancellationTokenSource();
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
            _cancelationSource.Cancel();
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
                    var cancelToken = _cancelationSource.Token;
                    if (_fileSeqence.Count == 0)
                    {
                        return;
                    }

                    HostLogger.Get<DocumentControlSystemService>().Info("Pdf generation started...");

                    using (var pdfFile = new PdfDocument())
                    {
                        var imageFiles = GetFiles(_fileSeqence);
                        foreach (var imageFile in imageFiles)
                        {
                            HostLogger.Get<DocumentControlSystemService>().Info($"Adding of file: {imageFile}");
                            var page = pdfFile.AddPage();
                            var gfx = XGraphics.FromPdfPage(page);
                            using (var image = XImage.FromFile(imageFile.ToString()))
                            {
                                var imageWidth = (double)(image.PixelWidth < page.Width ? image.PixelWidth : page.Width);
                                var imageHeight = (imageWidth / image.PixelWidth) * image.PixelHeight;
                                gfx.DrawImage(image, 0, 0, imageWidth, imageHeight);
                            }

                            if (cancelToken.IsCancellationRequested)
                            {
                                return;
                            }
                        }

                        HostLogger.Get<DocumentControlSystemService>().Info("Pdf generation finished...");
                        var resultPdfFile = Path.Combine(_processedDirectory,
                            $"{DateTime.Now.ToString("yyyy-MMMM-dd(HH-mm-ss)")}.pdf");
                        HostLogger.Get<DocumentControlSystemService>().Info($"Pdf file saving: \n {resultPdfFile}");
                        pdfFile.Save(resultPdfFile);
                    }

                    foreach (var imageFile in _fileSeqence.ToList())
                    {
                        var fileName = Path.GetFileName(imageFile);
                        if (fileName != null)
                        {
                            var pocessedFilePath = Path.Combine(_processedDirectory, fileName);
                            MoveWithRenaming(imageFile, pocessedFilePath);
                            _fileSeqence.Remove(imageFile);
                        }
                    }

                }
            }
            catch (Exception e)
            {
                HostLogger.Get<DocumentControlSystemService>().Error(e.Message);
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

            HostLogger.Get<DocumentControlSystemService>().Info($"Moving of file started:\n {sourceFilePath}\n ->\n {resultFilePath}");

            File.Move(sourceFilePath, resultFilePath);
        }

        private bool IsFileValid(string resultFilePath)
        {
            return Regex.IsMatch(resultFilePath, @".*[.](jpg|jpeg|png)$");
        }
    }
}
