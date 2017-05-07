using System.Collections.Generic;
using Multithreading.Enums;

namespace Multithreading.Models
{
    public class Queue
    {
        public const int DefaultNumber = 1;

        public Queue()
        {
            Number = DefaultNumber;
            Downloads = new List<Download>();
        }

        public int Number {get; set; }

        public List<Download> Downloads { get; set; }

        public string Description { get; set; }

        public string Name => Description ?? Number.ToString();

        public QueueType Type { get; set; }

        /// <summary>
        /// todo: checking of any download in progress
        /// </summary>
        public bool IsInProgress { get; }

        public Queue Clone()
        {
            return new Queue
            {
                Type = Type,
                Number = Number,
                Downloads = Downloads,
                Description = Description
            };
        }
    }
}
