using Castle.DynamicProxy;
using Topshelf;

namespace AOP
{
    static class Program
    {
        static void Main(string[] args)
        {
            ProxyGenerator generator = new ProxyGenerator();
            var service = generator.CreateClassProxy<DocumentControlSystemService>(new LoggingInterceptor());
            HostFactory.Run(x =>
            {
                x.Service<DocumentControlSystemService>(s =>
                {
                    s.ConstructUsing(host => service);
                    s.WhenStarted(instance => instance.Start());
                    s.WhenStopped(instance => instance.Stop());
                });
                x.SetServiceName("DCS");
                x.SetDisplayName("Document Control System");
                x.UseNLog();
                x.StartAutomaticallyDelayed();
                x.EnableServiceRecovery(r =>
                {
                    r.RestartService(1);
                    r.SetResetPeriod(1);
                });
            });
        }
    }
}
