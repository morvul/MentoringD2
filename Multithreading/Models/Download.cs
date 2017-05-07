using System;
using System.IO;
using System.Net;

namespace Multithreading.Models
{
    public class Download
    {
        public Download()
            : this(new Queue())
        {
        }

        public Download(Queue queue)
        {
            WebClient = new WebClient();
            Queue = queue;
        }

        public string ErrorMessage { get; set; }

        public double Progress { get; set; }

        public WebClient WebClient { get; }

        public bool HasError => ErrorMessage != null;

        public Uri SourcePath { get; set; }

        public string DestinationPath { get; set; }

        public string FileName { get; set; }

        public string DestinationalFile => Path.Combine(DestinationPath, FileName);

        public Queue Queue { get; set; }
    }
}
