using MessageQueue.RemoteController.Enums;

namespace MessageQueue.RemoteController.Models
{
    public class RemoteControlResult
    {
        public RemoteControlResultCode Code { get; set; }

        public string Message { get; set; }
    }
}
