using Topshelf;

namespace MessageQueue.ProcessingService
{
    static class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<ProcessingService>();
                x.SetServiceName("DCS Processing service");
                x.SetDisplayName("Document Control System - Processing service");
                x.UseNLog();
                x.StartManually();
            });
        }
    }
}
