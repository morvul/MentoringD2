using System;

namespace Profiling
{
    public class Item
    {
        private long[] _data;
        
        public Item(long[] data)
        {
            _data = data;
        }

        public void RegisterEvents(MemoryLeaksGenerator memoryLeaksGenerator)
        {
            memoryLeaksGenerator.OnEvent += SomeEventHandler;
        }

        private void SomeEventHandler()
        {
            Console.WriteLine("Event handler");
        }

        public void DoWork()
        {
            Console.WriteLine($"{DateTime.Now} Work");
        }
    }
}
