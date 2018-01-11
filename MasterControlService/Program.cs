using System;
using System.ServiceProcess;

namespace MasterControlService
{
    static class Program
    {
        static void Main()
        {
            MasterControl service = new MasterControl();

            if (Environment.UserInteractive)
            {
                Console.WriteLine("Starting service...");
                service.Start();
                Console.WriteLine("Service is running.");
                Console.WriteLine("Press any key to stop...");
                Console.ReadKey(true);
                Console.WriteLine("Stopping service...");
                service.Stop();
                Console.WriteLine("Service stopped.");
            }
            else
            {


                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new MasterControl()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
