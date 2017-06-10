using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Topshelf.Logging;

namespace AOP
{
    public class DocumentControlSystemService
    {
        private readonly CancellationTokenSource _cancelationSource;
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
            _cancelationSource = new CancellationTokenSource();
        }

        public bool Start()
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


        public bool Stop()
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
                        foreach (var imageFile in _fileSeqence)
                        {
                            HostLogger.Get<DocumentControlSystemService>().Info($"Adding of file: {imageFile}");
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

        [LoggingPostSharpAspect]
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

        protected virtual void NewFile(object sender, FileSystemEventArgs e)
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

        [LoggingPostSharpAspect]
        private void ProcessFile(string sourceFilePath, string fileName)
        {
            var resultFilePath = Path.Combine(_outputDirectory, fileName);
            if (sourceFilePath != resultFilePath)
            {
                resultFilePath = MoveWithRenaming(sourceFilePath, resultFilePath);
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

            _sequanceCountdown.Change(_sequanceTime, _sequanceTime);
            HostLogger.Get<DocumentControlSystemService>().Info("Success");
        }

        protected virtual string MoveWithRenaming(string sourceFilePath, string resultFilePath)
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
    }
}
