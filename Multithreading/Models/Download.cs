using System;
using System.Net;

namespace Multithreading.Models
{
    public class Download
    {
        public Download()
        {
            WebClient = new WebClient();
        }

        public string ErrorMessage { get; set; }

        public double Progress { get; set; }

        public WebClient WebClient { get; }

        public bool HasError => ErrorMessage != null;

        public Uri SourcePath { get; set; }

        public string DestinationPath { get; set; }

        public string FileName { get; set; }
    }
}
