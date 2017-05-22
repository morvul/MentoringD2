using MessageQueue.RemoteController.Enums;

namespace MessageQueue.RemoteController.Models
{
    public class RemoteControlCommand
    {
        public RemoteControlCommandCode Code { get; set; }

        public object Data { get; set; }
    }
}
