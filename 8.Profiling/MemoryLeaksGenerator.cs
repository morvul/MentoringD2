using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Profiling
{
    public class MemoryLeaksGenerator
    {
        private const int LeakObjSize = 100000;
        private const int LeakDelay = 1000;
        private readonly long[] _leakData;
        private readonly CancellationTokenSource _unmanagedCancellationTokenSource;
        private readonly CancellationTokenSource _managedCancellationTokenSource;

        public MemoryLeaksGenerator()
        {
            _leakData = new long[1000000];
            _unmanagedCancellationTokenSource = new CancellationTokenSource();
            _managedCancellationTokenSource = new CancellationTokenSource();
        }
        public void StartGenerateUnmanagedLeak()
        {
            Task.Run(() => UnmanagedLeakGeneration(_unmanagedCancellationTokenSource.Token));
        }

        public void StopGenerateUnmanagedLeak()
        {
            _unmanagedCancellationTokenSource.Cancel();
        }


        public void StartGenerateManagedLeak()
        {
            Task.Run(() => ManagedLeakGeneration(_managedCancellationTokenSource.Token));
        }

        public void StopGenerateManagedLeak()
        {
            _managedCancellationTokenSource.Cancel();
        }

        private void UnmanagedLeakGeneration(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                Marshal.AllocHGlobal(LeakObjSize);
                Thread.Sleep(LeakDelay);
            }
        }
        private void ManagedLeakGeneration(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                var itemWithEvent = new Item(_leakData);
                itemWithEvent.OnEvent += () => {};
                Thread.Sleep(LeakDelay);
            }
        }
    }
}
