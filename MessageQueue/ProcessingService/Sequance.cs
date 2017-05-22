using System;
using System.Collections.Generic;
using System.Threading;

namespace MessageQueue.ProcessingService
{
    public class Sequance
    {
        private readonly Guid _agentId;
        private readonly List<string> _fileSeqence;
        private readonly Timer _sequanceTimer;
        private CancellationToken _cancelationToken;
        private int _sequanceTime;


        public event Action<List<string>, Guid> OnSequanceCompleted;

        public Sequance()
        {
            _fileSeqence = new List<string>();
            _sequanceTimer = new Timer(SequanceCompleted);
        }

        private void SequanceCompleted(object state)
        {
            if (_cancelationToken.IsCancellationRequested)
            {
                _sequanceTimer.Dispose();
                return;
            }

            OnSequanceCompleted?.Invoke(_fileSeqence, _agentId);
        }

        public Sequance(Guid agentId, int sequanceTime, CancellationToken cancelationToken)
            : this()
        {
            _sequanceTime = sequanceTime;
            _agentId = agentId;
            _cancelationToken = cancelationToken;

        }

        public void AddSequanceItem(string filePath)
        {
            _fileSeqence.Add(filePath);
            _sequanceTimer.Change(_sequanceTime, _sequanceTime);
        }
    }
}
