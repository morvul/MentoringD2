using System.IO;
using System.Text.RegularExpressions;
using Topshelf.Logging;

namespace MessageQueue.FileMonitorService
{
    public static class FileHelper
    {

        public static string MoveWithRenaming(string sourceFilePath, string resultFilePath)
        {
            resultFilePath = GetUniqueName(sourceFilePath, resultFilePath);
            HostLogger.Get<DocumentControlSystemService>().Info($"Moving of file started:\n {sourceFilePath}\n ->\n {resultFilePath}");

            File.Move(sourceFilePath, resultFilePath);
            return resultFilePath;
        }

        public static string GetUniqueName(string outputDirectory, string fileName)
        {
            var resultFilePath = Path.Combine(outputDirectory, fileName);
            var directory = Path.GetDirectoryName(resultFilePath) ?? "";
            var ext = Path.GetExtension(fileName) ?? "";
            while (File.Exists(resultFilePath))
            {
                fileName += " - Copy";
                resultFilePath = Path.Combine(directory, fileName + ext);
            }

            return resultFilePath;
        }

        public static bool IsFileValid(string resultFilePath)
        {
            return Regex.IsMatch(resultFilePath, @".*[.](jpg|jpeg|png)$");
        }
    }
}
