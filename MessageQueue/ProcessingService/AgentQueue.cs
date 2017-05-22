using System;

namespace MessageQueue.ProcessingService
{
    public class AgentQueue
    {
        public string QueueName { get; set; }

        public Guid AgentId { get; set; }
    }
}
