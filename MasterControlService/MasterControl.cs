using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Timers;
using NLog;
using MasterModuleCommon;
using ScreenConnection;
using System.Xml.Serialization;
using MasterControlService.Config;
using MasterControlService.Web;
using Nancy.Hosting.Self;

namespace MasterControlService
{
    public enum ServiceState
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ServiceStatus
    {
        public int dwServiceType;
        public ServiceState dwCurrentState;
        public int dwControlsAccepted;
        public int dwWin32ExitCode;
        public int dwServiceSpecificExitCode;
        public int dwCheckPoint;
        public int dwWaitHint;
    };

    public partial class MasterControl : ServiceBase
    {
        private Logger allLogger;
        private Logger serviceLogger;
        private Logger extScreenLogger;
        private Logger webLogger;

        public MasterControl()
        {
            InitializeComponent();

            allLogger = LogManager.GetLogger("AllLog");
            serviceLogger = LogManager.GetLogger("ServiceLog");
            extScreenLogger = LogManager.GetLogger("ExtScreenLog");
            webLogger = LogManager.GetLogger("WebLog");


        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        public void Start()
        {
            OnStart(new string[0]);
        }

        public NancyHost WebServer;

        public List<Type> GraphicsTypes = new List<Type>();
        public List<Type> TargetsTypes = new List<Type>();
        public List<Type> TransitionsTypes = new List<Type>();

        public Dictionary<string, Screen> screens = new Dictionary<string, Screen>();

        protected override void OnStart(string[] args)
        {
            allLogger.Info("");
            allLogger.Info("");
            allLogger.Info("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
            allLogger.Info("=-=-=-=-=-=-=-=-=-=- Master Control Service =-=-=-=-=-=-=-=-=-=-=");
            allLogger.Info("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
            allLogger.Info("");
            allLogger.Info("Initializing At " + DateTime.Now.ToString("O"));
            allLogger.Info("");

            // Update the service state to Start Pending.  
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            // Listing and importing all modules
            LoadModules(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Modules\"));

            // Connection to external screen
            InitExtScreen();


            // Init Http Listener
            webLogger.Info("Starting web server...");

            HostConfiguration hostConfigs = new HostConfiguration
            {
                UrlReservations =
                {
                    CreateAutomatically = true
                }
            };

            HttpServerStatic.WebLogger = webLogger;
            Uri webUri = new Uri("http://localhost:80");
            webLogger.Info("Starting web server on " + webUri.ToString());

            WebServer = new NancyHost(hostConfigs, webUri);
            WebServer.Start();

            webLogger.Info("Started !");

            // 
            serviceLogger.Info("Scanning for screens...");

            screens = ScreenConnection.Connector.Discovery(1000);

            foreach (System.Collections.Generic.KeyValuePair<string, Screen> kvp in screens)
            {
                serviceLogger.Info("Found a screen \"" + kvp.Value.Id + "\" at " + kvp.Value.Ip + ":" + kvp.Value.Port);
            }




            // Update the service state to Running.  
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);



            /*


            MasterConfig masterConfig = new MasterConfig();
            masterConfig.Screens.Add(new ScreenDescriptor()
            {
                Id = "scr-01",
                X = 10,
                Y = 10,
                Rotation = Rotation.DEG_0
            });
            masterConfig.Routines.Add(new Routine()
            {
                Id = 1,
                GraphicProvider = new GraphicProviderDescriptor()
                {
                    ProviderName = GraphicsTypes.First().Name,
                    Parameters = new List<MasterModuleCommon.KeyValuePair<string, string>>()
                    {
                        new MasterModuleCommon.KeyValuePair<string, string>("paramName","valuuue"),
                        new MasterModuleCommon.KeyValuePair<string, string>("paramName2","valuuue222")
                    }
                }
            });
            masterConfig.Sequence.Add(new Stage()
            {
                RoutineId = 1,
                RepeatCount = 10,
                PostExecDelay = 10000
            });


            XmlSerializer xs = new XmlSerializer(typeof(MasterConfig));
            using (StreamWriter wr = new StreamWriter(@"C:\MasterControl\Config.xml"))
            {
                xs.Serialize(wr, masterConfig);
            }

            GraphicProvider provider = (GraphicProvider)Activator.CreateInstance(GraphicsTypes.First(item => item.Name == "DrawfriendPonyGraphicsProvider"));

            provider.Init(logger, new List<MasterModuleCommon.KeyValuePair<string, string>>());


            */

            return;




            /*

            foreach (Type graphicsType in GraphicsTypes)
            {
                GraphicProvider provider = (GraphicProvider)Activator.CreateInstance(graphicsType);

                provider.Init(new Dictionary<string, object>());

                foreach (Rotation rot in Enum.GetValues(typeof(Rotation)))
                {
                    Screen screen = new Screen("0.0.0.0")
                    {
                        Rotation = rot
                    };

                    Bitmap result = provider.GetNextGraphic(screen);

                    result.Save(@"C:\MasterControl\TestImages\out" + Enum.GetName(typeof(Rotation), rot) + ".png", ImageFormat.Png);
                }

            }*/

        }

        protected override void OnStop()
        {
            allLogger.Info("Stopping...");
            allLogger.Info("");
            allLogger.Info("");
            allLogger.Info("");

            LogManager.Flush();
            LogManager.Shutdown();
        }

        private void LoadModules(string rootModuleFolder)
        {
            serviceLogger.Info("======== Module Import ========");
            serviceLogger.Info("---- Graphics Modules Import ----");

            string lookupFolder = Path.Combine(rootModuleFolder, @"Graphics\");

            serviceLogger.Info("Looking for graphics dll modules in \"" + lookupFolder + "\"");
            GraphicsTypes = new List<Type>();

            if (!Directory.Exists(lookupFolder))
            {
                serviceLogger.Warn("    = Folder don't exist !");
            }
            else
            {
                foreach (string dllPath in Directory.GetFiles(lookupFolder, "*.dll"))
                {
                    serviceLogger.Info("    = Loading dll : " + dllPath);
                    Assembly asm = Assembly.LoadFrom(dllPath);

                    foreach (Type type in asm.GetTypes().Where(myType =>
                        myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(GraphicProvider))))
                    {
                        serviceLogger.Info("        - Found \"" + type.Name + "\"");
                        GraphicsTypes.Add(type);
                    }
                }
            }

            serviceLogger.Info("---- Targets Modules Import ----");

            lookupFolder = Path.Combine(rootModuleFolder, @"Targets\");

            serviceLogger.Info("Looking for targets dll modules in \"" + lookupFolder + "\"");
            TargetsTypes = new List<Type>();

            if (!Directory.Exists(lookupFolder))
            {
                serviceLogger.Warn("    = Folder don't exist !");
            }
            else
            {
                foreach (string dllPath in Directory.GetFiles(lookupFolder, "*.dll"))
                {
                    serviceLogger.Info("    = Loading dll : " + dllPath);
                    Assembly asm = Assembly.LoadFrom(dllPath);

                    foreach (Type type in asm.GetTypes().Where(myType =>
                        myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(TargetProvider))))
                    {
                        serviceLogger.Info("        - Found \"" + type.Name + "\"");
                        TargetsTypes.Add(type);
                    }
                }
            }

            serviceLogger.Info("---- Transitions Modules Import ----");

            lookupFolder = Path.Combine(rootModuleFolder, @"Transitions\");

            serviceLogger.Info("Looking for transitions dll modules in \"" + lookupFolder + "\"");
            TransitionsTypes = new List<Type>();

            if (!Directory.Exists(lookupFolder))
            {
                serviceLogger.Warn("    = Folder don't exist !");
            }
            else
            {
                foreach (string dllPath in Directory.GetFiles(lookupFolder, "*.dll"))
                {
                    serviceLogger.Info("    = Loading dll : " + dllPath);
                    Assembly asm = Assembly.LoadFrom(dllPath);

                    foreach (Type type in asm.GetTypes().Where(myType =>
                        myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(TransitionProvider))))
                    {
                        serviceLogger.Info("        - Found \"" + type.Name + "\"");
                        TransitionsTypes.Add(type);
                    }
                }
            }
        }

        private ExtScreenSerial serial;
        private Timer serialInitTimer = new Timer();

        private void InitExtScreen()
        {
            extScreenLogger.Info("======== External Screen Initialization ========");

            serial = new ExtScreenSerial(extScreenLogger);

            serialInitTimer.Interval = 5000;
            serialInitTimer.AutoReset = false;
            serialInitTimer.Elapsed += SerialInitTimer_Elapsed;

            SerialInitTimer_Elapsed(null, null);
        }

        private void SerialInitTimer_Elapsed(object sender, ElapsedEventArgs e)
        {

            extScreenLogger.Info("Trying to connect External Screen");
            serial = new ExtScreenSerial(extScreenLogger);
            if (serial.Locate())
            {
                serialInitTimer.Stop();
                extScreenLogger.Info("External screen found on COM port " + serial.SerilPortName);
                serial.Connect();
                serial.DataReceived += Serial_DataReceived;
            }
            else
            {
                extScreenLogger.Warn("External screen not found, retrying in 5 second");
                serialInitTimer.Start();
            }
        }


        private void Serial_DataReceived(string request)
        {
            string[] requestSplitted = request.Split(';');

            extScreenLogger.Info("Request received : " + request);

            switch (requestSplitted[0])
            {
                case "AreYouStillThere":
                    {
                        extScreenLogger.Info("Alive");
                        serial.Send("Yes");
                        break;
                    }
                case "GetScreens":
                    {
                        int pageSize = 4;
                        int pageNb = Convert.ToInt32(requestSplitted[1]);

                        List<string> output = new List<string>();

                        string[] searchArgs = String.Join(";", requestSplitted.Skip(2)).ToLowerInvariant().Split(' ').ToArray();

                        List<MasterModuleCommon.KeyValuePair<string, Screen>> screenList = screens.Select(kvp => new MasterModuleCommon.KeyValuePair<string, Screen>("".ToLowerInvariant(), kvp.Value))
                                                                                                  .ToList();
                        if (searchArgs.Length != 0)
                        {
                            screenList = screenList.Where(item => searchArgs.Any(serchedVal => item.Key.Contains(serchedVal))).ToList();
                        }

                        output.Add((pageNb + 1).ToString());
                        output.Add(Math.Ceiling(screenList.Count / (double)pageSize).ToString());
                        output.Add(String.Join(" ", searchArgs));



                        screenList = screenList.Skip(pageNb * pageSize).Take(pageSize).ToList();



                        for (int i = 0; i < pageSize; i++)
                        {
                            if (screenList.Count() < i)
                            {
                                output.Add("");
                                output.Add("NoIp");
                            }
                            else
                            {
                                output.Add(screenList[i].Key);
                                output.Add(screenList[i].Value.Ip);
                            }

                        }

                        serial.Send(output);

                        break;
                    }

            }

            
        }


    }
}
