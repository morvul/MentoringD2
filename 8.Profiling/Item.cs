using System;

namespace Profiling
{
    public class Item
    {
        private long[] _data;
        public event Action OnEvent;

        public Item(long[] data)
        {
            _data = data;
        }

        public void Action()
        {
            OnEvent?.Invoke();
        }
    }
}
