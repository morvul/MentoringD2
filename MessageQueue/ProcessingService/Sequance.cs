using System.Collections.Generic;

namespace MessageQueue.ProcessingService
{
    public class Sequance
    {
        public List<string> FileSeqence { get; private set; }

        public Sequance()
        {
            FileSeqence = new List<string>();
        }
    }
}
