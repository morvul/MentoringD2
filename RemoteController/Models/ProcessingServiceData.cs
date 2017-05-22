namespace MessageQueue.RemoteController.Models
{
    public class ProcessingServiceData
    {
        public string OutputDirectory { get; set; }

        public string TrashDirectory { get; set; }

        public int SequanceTime { get; set; }

        public string Status { get; set; }
    }
}
