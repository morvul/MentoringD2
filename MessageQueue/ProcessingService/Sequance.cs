using System;
using System.Threading;

namespace MessageQueue.ProcessingService
{
    public class Sequance
    {
        private readonly Guid _agentId;
        private readonly Timer _sequanceTimer;
        private CancellationToken _cancelationToken;
        private int _sequanceTime;


        public event Action<Guid, CancellationToken> OnSequanceCompleted;

        public Sequance()
        {
            _sequanceTimer = new Timer(SequanceCompleted);
        }

        private void SequanceCompleted(object state)
        {
            if (_cancelationToken.IsCancellationRequested)
            {
                _sequanceTimer.Dispose();
                return;
            }

            OnSequanceCompleted?.Invoke(_agentId, _cancelationToken);
        }

        public Sequance(Guid agentId, int sequanceTime, CancellationToken cancelationToken)
            : this()
        {
            _sequanceTime = sequanceTime;
            _agentId = agentId;
            _cancelationToken = cancelationToken;

        }

        public void UpdateSequanceState()
        {
            _sequanceTimer.Change(_sequanceTime, _sequanceTime);
        }

        public void UpdateSequanceSettings(int sequanceTime)
        {
            _sequanceTime = sequanceTime;
            UpdateSequanceState();
        }
    }
}
