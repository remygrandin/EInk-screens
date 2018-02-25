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
        private Logger logger;

        public MasterControl()
        {
            InitializeComponent();

            logger = LogManager.GetLogger("GlobalLog");



        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        public void Start()
        {
            OnStart(new string[0]);
        }

        public List<Type> GraphicsTypes = new List<Type>();
        public List<Type> TargetsTypes = new List<Type>();
        public List<Type> TransitionsTypes = new List<Type>();

        public Dictionary<string, Screen> screens = new Dictionary<string, Screen>();

        protected override void OnStart(string[] args)
        {
            logger.Info("");
            logger.Info("");
            logger.Info("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
            logger.Info("=-=-=-=-=-=-=-=-=-=- Master Control Service =-=-=-=-=-=-=-=-=-=-=");
            logger.Info("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
            logger.Info("");

            // Update the service state to Start Pending.  
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            // Listing and importing all modules
            LoadModules(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Modules\"));

            // Connection to external screen
            InitExtScreen();

            logger.Info("Scanning for screens...");

            screens = ScreenConnection.Connector.Discovery(1000);

            foreach (System.Collections.Generic.KeyValuePair<string, Screen> kvp in screens)
            {
                logger.Info("Found a screen \"" + kvp.Value.Id + "\" at " + kvp.Value.Ip + ":" + kvp.Value.Port);
            }

            foreach (System.Collections.Generic.KeyValuePair<string, Screen> kvp in screens)
            {
                logger.Info("Found a screen \"" + kvp.Value.Id + "\" at " + kvp.Value.Ip + ":" + kvp.Value.Port);
            }




            // Update the service state to Running.  
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);





            
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

        private void LoadModules(string rootModuleFolder)
        {
            logger.Info("======== Module Import ========");
            logger.Info("---- Graphics Modules Import ----");

            string lookupFolder = Path.Combine(rootModuleFolder, @"Graphics\");

            logger.Info("Looking for graphics dll modules in \"" + lookupFolder + "\"");
            GraphicsTypes = new List<Type>();

            if (!Directory.Exists(lookupFolder))
            {
                logger.Warn("    = Folder don't exist !");
            }
            else
            {
                foreach (string dllPath in Directory.GetFiles(lookupFolder, "*.dll"))
                {
                    logger.Info("    = Loading dll : " + dllPath);
                    Assembly asm = Assembly.LoadFrom(dllPath);

                    foreach (Type type in asm.GetTypes().Where(myType =>
                        myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(GraphicProvider))))
                    {
                        logger.Info("        - Found \"" + type.Name + "\"");
                        GraphicsTypes.Add(type);
                    }
                }
            }

            logger.Info("---- Targets Modules Import ----");

            lookupFolder = Path.Combine(rootModuleFolder, @"Targets\");

            logger.Info("Looking for targets dll modules in \"" + lookupFolder + "\"");
            TargetsTypes = new List<Type>();

            if (!Directory.Exists(lookupFolder))
            {
                logger.Warn("    = Folder don't exist !");
            }
            else
            {
                foreach (string dllPath in Directory.GetFiles(lookupFolder, "*.dll"))
                {
                    logger.Info("    = Loading dll : " + dllPath);
                    Assembly asm = Assembly.LoadFrom(dllPath);

                    foreach (Type type in asm.GetTypes().Where(myType =>
                        myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(TargetProvider))))
                    {
                        logger.Info("        - Found \"" + type.Name + "\"");
                        TargetsTypes.Add(type);
                    }
                }
            }

            logger.Info("---- Transitions Modules Import ----");

            lookupFolder = Path.Combine(rootModuleFolder, @"Transitions\");

            logger.Info("Looking for transitions dll modules in \"" + lookupFolder + "\"");
            TransitionsTypes = new List<Type>();

            if (!Directory.Exists(lookupFolder))
            {
                logger.Warn("    = Folder don't exist !");
            }
            else
            {
                foreach (string dllPath in Directory.GetFiles(lookupFolder, "*.dll"))
                {
                    logger.Info("    = Loading dll : " + dllPath);
                    Assembly asm = Assembly.LoadFrom(dllPath);

                    foreach (Type type in asm.GetTypes().Where(myType =>
                        myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(TransitionProvider))))
                    {
                        logger.Info("        - Found \"" + type.Name + "\"");
                        TransitionsTypes.Add(type);
                    }
                }
            }
        }

        private ExtScreenSerial serial;
        private Timer serialInitTimer = new Timer();

        private void InitExtScreen()
        {
            serial = new ExtScreenSerial(logger);

            serialInitTimer.Interval = 5000;
            serialInitTimer.AutoReset = false;
            serialInitTimer.Elapsed += SerialInitTimer_Elapsed;

            SerialInitTimer_Elapsed(null, null);


        }

        private void SerialInitTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            logger.Info("======== External Screen Initialization ========");
            serial = new ExtScreenSerial(logger);
            if (serial.Locate())
            {
                serial.Connect();
                serial.DataReceived += Serial_DataReceived;
                serialInitTimer.Stop();
            }
            else
            {
                logger.Info("External screen not found, retrying in 5 second");
                serialInitTimer.Start();
            }
        }


        private void Serial_DataReceived(string request)
        {
            string[] requestSplitted = request.Split(';');

            switch (requestSplitted[0])
            {
                case "GetScreens":
                {
                    int pageSize = 4;
                    int pageNb = Convert.ToInt32(requestSplitted[1]);

                    string[] searchArgs = String.Join(";", requestSplitted.Skip(2)).ToLowerInvariant().Split(' ').ToArray();

                    IEnumerable<MasterModuleCommon.KeyValuePair<string, Screen>> screenList = 
                        screens.Select(kvp => new MasterModuleCommon.KeyValuePair<string, Screen>("".ToLowerInvariant(), kvp.Value));

                    if (searchArgs.Length != 0)
                    {
                        screenList = screenList.Where(item =>
                            searchArgs.Any(serchedVal => item.Key.Contains(serchedVal)));
                    }

                    screenList = screenList.Skip(pageNb * pageSize).Take(pageSize);

                    List<string> output = new List<string>();
                    for (int i = 0; i < pageSize; i++)
                    {

                    }



                    serial.Send("testtest");

                    break;
                }

            }

            logger.Info("Request received : " + request);
        }

        protected override void OnStop()
        {
            logger.Info("Stopping...");
        }
    }
}
