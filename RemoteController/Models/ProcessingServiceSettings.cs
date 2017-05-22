namespace MessageQueue.RemoteController.Models
{
    public class ProcessingServiceSettings
    {
        public string OutputDirectory { get; set; }

        public string TrashDirectory { get; set; }

        public int SequanceTime { get; set; }
    }
}
