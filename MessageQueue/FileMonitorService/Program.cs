using Topshelf;

namespace MessageQueue.FileMonitorService
{
    static class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<DocumentControlSystemService>();
                x.SetServiceName("DCS File Monitor");
                x.SetDisplayName("Document Control System - File Monitor");
                x.UseNLog();
                x.StartAutomaticallyDelayed();
            });
        }
    }
}
