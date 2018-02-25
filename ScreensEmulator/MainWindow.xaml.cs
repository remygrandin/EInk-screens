using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using ScreenConnection;

namespace ScreensEmulator
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static int screenDiscoveryPort = 2501;


        public UdpClient discoveryServer;

        public MainWindow()
        {

            InitializeComponent();

            IPEndPoint all = new IPEndPoint(IPAddress.Any, screenDiscoveryPort);
            discoveryServer = new UdpClient(all);

            discoveryServer.EnableBroadcast = true;
            discoveryServer.BeginReceive(RequestCallback, null);

            CreateScreen(null, null);
            CreateScreen(null, null);
            CreateScreen(null, null);
            CreateScreen(null, null);
            CreateScreen(null, null);
        }

        private void RequestCallback(IAsyncResult ar)
        {
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, screenDiscoveryPort);

            byte[] buffer = discoveryServer.EndReceive(ar, ref sender);

            if (tblScreens.Items.Count != 0)
            {
                // format : |DISCOVERY_RESPONSE;[device TCP IP];[device TCP port];[device MAC];[device ID]|
                string resposeStr = String.Join("|", tblScreens.Items.OfType<ScreenBase>().Select(item =>
                        "DISCOVERY_RESPONSE;" + item.Ip + ";" + item.Port + ";" + item.Mac + ";" + item.Id));

                var outputBuffer = Encoding.ASCII.GetBytes(resposeStr);

                discoveryServer.Send(outputBuffer, outputBuffer.Length, sender);
            }

            discoveryServer.BeginReceive(RequestCallback, null);
        }
    
        

        private void ShowScreenMonitor(object sender, RoutedEventArgs e)
        {
            Button btn = (Button) sender;

            ScreenBase screen = (ScreenBase) btn.DataContext;

            if (screens[screen].Visibility == Visibility.Hidden)
                screens[screen].Visibility = Visibility.Visible;
            else
                screens[screen].Visibility = Visibility.Hidden;

        }


        private int currentPort = 30000;

        private Dictionary<ScreenBase, ScreenMonitor> screens = new Dictionary<ScreenBase, ScreenMonitor>();

        private void CreateScreen(object sender, RoutedEventArgs e)
        {
            int port = currentPort;
            currentPort++;

            ScreenBase screen = new ScreenBase()
            {
                Ip="192.168.1.185",
                Mac = "00:11:22:33:44:55",
                Id="Fake-Screen-" + port,
                Port = port
            };

            ScreenMonitor monitor = new ScreenMonitor(screen);
            monitor.Visibility = Visibility.Hidden;

            screens.Add(screen, monitor);

            tblScreens.Items.Add(screen);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (KeyValuePair<ScreenBase, ScreenMonitor> keyValuePair in screens)
            {
                keyValuePair.Value.server.Stop();
                keyValuePair.Value.Close();
            }

            Thread.CurrentThread.Abort();
        }
    }
}
