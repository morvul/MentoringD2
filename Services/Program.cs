using NLog;
using NLog.Config;
using Topshelf;

namespace Services
{
    static class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<DocumentControlSystemService>();
                x.SetServiceName("DCS");
                x.SetDisplayName("Document Control System");
                x.UseNLog();
                x.StartAutomaticallyDelayed();
            });
        }
    }
}
