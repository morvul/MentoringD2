using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Profiling
{
    public class MemoryLeaksGenerator
    {
        private const int LeakObjSize = 1000000;
        private const int LeakDelay = 100;
        private CancellationTokenSource _unmanagedCancellationTokenSource;
        private CancellationTokenSource _managedCancellationTokenSource;

        public void StartGenerateUnmanagedLeak()
        {
            _unmanagedCancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => UnmanagedLeakGeneration(_unmanagedCancellationTokenSource.Token));
        }

        public void StopGenerateUnmanagedLeak()
        {
            _unmanagedCancellationTokenSource.Cancel();
        }


        public void StartGenerateManagedLeak()
        {
            _managedCancellationTokenSource = new CancellationTokenSource();
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
                var itemWithEvent = new Item(new long[LeakObjSize]);
                itemWithEvent.OnEvent += EventHandler;
                Thread.Sleep(LeakDelay);
            }
        }

        private void EventHandler()
        {
            if (_managedCancellationTokenSource.IsCancellationRequested)
            {
                MessageBox.Show("Item event!");
            }
        }
    }
}
