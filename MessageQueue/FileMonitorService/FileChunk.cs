using System;

namespace MessageQueue.FileMonitorService
{
    public class FileChunk
    {
        public long FilePosition { get; set; }

        public long FileSize { get; set; }

        public byte[] Data { get; set; }

        public int Size { get; set; }

        public string FileName { get; set; }

        public Guid AgentId { get; set; }
    }
}
