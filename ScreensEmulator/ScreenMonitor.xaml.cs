using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using FastBitmapLib;
using GrayScaleConverterLib;
using ScreenConnection;

namespace ScreensEmulator
{
    /// <summary>
    /// Logique d'interaction pour ScreenMonitor.xaml
    /// </summary>
    public partial class ScreenMonitor : Window
    {
        public static string endToken = "ENDOFTRANSMISSION";
        public static string packetStartToken = "STARTTRANSMISSION";

        public ScreenBase Screen;

        public byte[] screenBuffer = new byte[ScreenConnection.Screen.Width * ScreenConnection.Screen.Height];
        public byte grayScaleDepth = 8;

        public TcpListener server;

        public void log(string message)
        {
            DateTime now = DateTime.Now;
            string prefix = "[" + now.ToString("HH:mm:ss.fffffff") + "] ";

            this.Dispatcher.Invoke((Action)(() =>
            {
                txtbLog.AppendText(prefix + message + "\r\n");
            }));
        }

        public class actionEntry
        {
            public string time { get; set; }
            public string action { get; set; }
        }

        public void timeAction(string action)
        {
            var item = new actionEntry() { time = DateTime.Now.ToString("HH:mm:ss.fffffff"), action = action };

            this.Dispatcher.Invoke((Action)(() =>
            {
                tblActions.Items.Add(item);
            }));
        }

        public ScreenMonitor(ScreenBase screen)
        {
            Screen = screen;

            InitializeComponent();

            lblId.Content = Screen.Id;
            lblPort.Content = Screen.Port;

            this.Title += " - " + Screen.Id + " - " + Screen.Port;

            txtbLog.Text = "";

            log("Starting Screen ...");

            IPEndPoint ep = new IPEndPoint(IPAddress.Any, Screen.Port);

            server = new TcpListener(ep);

            server.Start();


            log("Waiting for a connection... ");

            server.BeginAcceptTcpClient(Callback, null);


        }

        private void Callback(IAsyncResult ar)
        {
            try
            {
                TcpClient client = server.EndAcceptTcpClient(ar);
                IPEndPoint clientEP = (IPEndPoint)client.Client.RemoteEndPoint;
                log("Client Connected (" + clientEP.Address + ":" + clientEP.Port + ")");

                // Get a stream object for reading and writing
                Stream stream = client.GetStream();

                bool allReceived = false;

                List<byte> data = new List<byte>();

                while (!allReceived)
                {
                    int readBytes;

                    byte[] subBuffer = new byte[1024 * 64];

                    // Loop to receive all the data sent by the client.
                    while ((readBytes = stream.Read(subBuffer, 0, subBuffer.Length)) != 0)
                    {
                        log("Received " + readBytes + " byte(s)");

                        data.AddRange(subBuffer.Take(readBytes).ToArray());

                        if (data.Count >= endToken.Length + 5)
                        {
                            if (System.Text.Encoding.ASCII.GetString(data.ToArray()).EndsWith(endToken))
                            {
                                log("End token received, decoding message ...");

                                byte actionId = data[0];
                                data.RemoveAt(0);

                                byte[] sizeArray = data.Take(4).ToArray();
                                data.RemoveAt(3);
                                data.RemoveAt(2);
                                data.RemoveAt(1);
                                data.RemoveAt(0);

                                int size = BitConverter.ToInt32(sizeArray, 0);

                                /* Debug
                                log("Server Request :");
                                log("   Action : " + actionId);
                                log("   Data Length : " + size);
                                log("   Data : ");
                                if (size < 50)
                                {
                                    log("       " + System.Text.Encoding.ASCII.GetString(data.ToArray()));
                                }
                                else
                                {
                                    log("        Response too long to display");
                                }

                                log("   Data (HEX) : ");
                                if (size < 50)
                                {
                                    string hexMessage = String.Join(" ", data.Select(item => item.ToString("X2")));
                                    log("       " + hexMessage);
                                }
                                else
                                {
                                    log("        Response too long to display");
                                }

    */

                                ProcessRequest(client, actionId, data.ToList());

                                data.Clear();
                            }
                        }

                    }



                }


            }
            catch (Exception e)
            {
                log(e.Message);
            }
            finally
            {
                try
                {
                    server.BeginAcceptTcpClient(Callback, null);
                }
                catch (Exception e)
                {

                }

            }
        }

        public void ProcessRequest(TcpClient client, byte actionId, List<byte> data)
        {
            log("Start of processing request");
            log("Simulating latency of 10ms");
            Thread.Sleep(10);
            switch (actionId)
            {
                // -- 01 : Ping --
                case 1:
                    {
                        timeAction("01 : Ping");
                        log("Server requested Ping");
                        Write(client, "Pong");
                        break;
                    }

                // -- 02 : Echo --
                case 2:
                    {
                        timeAction("02 : Echo");
                        log("Server requested Echo");
                        Write(client, data.ToArray());
                        break;
                    }

                // -- 03 : GetIp --
                case 3:
                    {
                        timeAction("03 : GetIp");
                        log("Server requested IP");
                        Write(client, Screen.Ip);
                        break;
                    }

                // -- 04 : GetMac --
                case 4:
                    {
                        timeAction("04 : GetMac");
                        log("Server requested MAC");
                        Write(client, Screen.Mac);
                        break;
                    }

                // -- 07 : Diagnostic Screen --
                case 7:
                    {
                        timeAction("07 : Diag");
                        log("Server requested Diag Screen");
                        break;
                    }

                // -- 08 : Test Debit --
                case 8:
                    {
                        timeAction("08 : Throughtput");
                        log("Server requested Throughtput test");
                        Write(client, "OK");
                        break;
                    }

                // -- 11 : GetId --
                case 11:
                    {
                        timeAction("11 : GetId");
                        log("Server requested ID");
                        Write(client, Screen.Id);
                        break;
                    }

                // -- 12 : SetId --
                case 12:
                    {
                        timeAction("12 : SetId");
                        string newId = System.Text.Encoding.ASCII.GetString(data.ToArray());
                        log("Server defined ID to \"" + newId + "\"");
                        Screen.Id = newId;
                        break;
                    }

                // -- 13 : ResetId --
                case 13:
                    {
                        timeAction("13 : ResetId");
                        log("Server requested ID reset \"" + Screen.Mac + "\"");
                        Screen.Id = Screen.Mac;
                        break;
                    }

                // -- 14 : Reboot --
                case 14:
                    {
                        timeAction("14 : Reboot");
                        log("Server requested Reboot");
                        break;
                    }

                // -- 15 : Shutdown --
                case 15:
                    {
                        timeAction("15 : Shutdown");
                        log("Server requested Shutdown");
                        break;
                    }

                // -- 21 : GetAllRegisters --
                case 21:
                    {
                        timeAction("21 : GetAllRegisters");
                        log("Server requested All Registers");
                        Write(client, Enumerable.Repeat<byte>(0, 18).ToArray());
                        break;
                    }

                // -- 22 : SetAllRegisters (17 bytes) --
                case 22:
                    {
                        timeAction("22 : SetAllRegisters");
                        log("Server requested setting All Registers");
                        Write(client, Enumerable.Repeat<byte>(0, 18).ToArray());
                        break;
                    }

                // -- 23 : GetRegister (1 byte address) --
                case 23:
                    {
                        timeAction("23 : GetRegister");
                        log("Server requested Registers " + data[0].ToString("X"));
                        Write(client, new byte[] { 0 });
                        break;
                    }

                // -- 24 : SetRegister (1 byte address, 1 byte value) --
                case 24:
                    {
                        timeAction("24 : SetRegister");
                        log("Server requested setting Registers " + data[0].ToString("X") + " to " + data[1].ToString("X"));
                        Write(client, new byte[] { 0 });
                        break;
                    }

                // ==== 3X : Power Function ====
                // -- 31 : Get Status --
                case 31:
                    {
                        timeAction("31 : Get Status");
                        log("Server requested power status");

                        Write(client, new byte[] { 0 });
                        break;
                    }

                // -- 32 : Power On --
                case 32:
                    {
                        timeAction("32 : Power On");
                        log("Server requested power on");

                        break;
                    }

                // -- 33 : Power Off --
                case 33:
                    {
                        timeAction("33 : Power Off");
                        log("Server requested power Off");

                        break;
                    }

                // -- 34 : Toggle Power --
                case 34:
                    {
                        timeAction("34 : Toggle Power");
                        log("Server requested power toggle");

                        break;
                    }

                // ==== 4X : Power Adjust ====
                // -- 41 : Get VCOM --
                case 41:
                    {
                        timeAction("41 : Get VCOM");
                        log("Server requested VCOM");
                        Write(client, new byte[] { 0 });

                        break;
                    }

                // -- 42 : Set VCOM --
                case 42:
                    {
                        timeAction("42 : Set VCOM");
                        log("Server requested setting VCOM to " + data[0]);

                        break;
                    }

                // -- 43 : Get VADJ --
                case 43:
                    {
                        timeAction("43 : Get VADJ");
                        log("Server requested VADJ");
                        Write(client, BitConverter.GetBytes(15000 - (1 - 3) * 250));

                        break;
                    }

                // -- 44 : Set VADJ --
                case 44:
                    {
                        timeAction("44 : Set VADJ");
                        log("Server requested setting VADJ to " + BitConverter.ToInt32(data.ToArray(), 0));

                        break;
                    }

                // ==== 5X : Temperature function ====
                // -- 51 : Get Temperature --
                case 51:
                    {
                        timeAction("51 : Get Temperature");
                        log("Server requested teperature");
                        Write(client, new byte[] { 0 });

                        break;
                    }

                // -- 52 : Get TCOLD --
                case 52:
                    {
                        timeAction("52 : Get TCOLD");
                        log("Server requested TCOLD");
                        Write(client, BitConverter.GetBytes(-7));

                        break;
                    }

                // -- 53 : Set TCOLD --
                case 53:
                    {
                        timeAction("53 : Set TCOLD");
                        log("Server requested setting TCOLD to " + BitConverter.ToInt32(data.ToArray(), 0));

                        break;
                    }

                // -- 54 : Get THOT --
                case 54:
                    {
                        timeAction("54 : Get THOT");
                        log("Server requested THOT");
                        Write(client, BitConverter.GetBytes(42));

                        break;
                    }

                // -- 55 : Set THOT --
                case 55:
                    {
                        timeAction("55 : Get THOT");
                        log("Server requested setting TCOLD to " + BitConverter.ToInt32(data.ToArray(), 0));

                        break;
                    }

                // ==== 6X : Power Up/Down Sequence & Timing ====
                // -- 61 : Get Power Up Sequence --
                case 61:
                    {
                        timeAction("61 : Get Power Up Sequence");
                        log("Server requested Power Up Sequence");
                        Write(client, new byte[] { 0, 0, 0, 0 });

                        break;
                    }

                // -- 62 : Set Power Up Sequence --
                case 62:
                    {
                        timeAction("62 : Set Power Up Sequence");
                        log("Server requested setting Power Up Sequence to " + String.Join(" ", data.Select(item => item.ToString("X2"))));

                        break;
                    }

                // -- 63 : Get Power Down Sequence --
                case 63:
                    {
                        timeAction("63 : Get Power Down Sequence");
                        log("Server requested Power Down Sequence");
                        Write(client, new byte[] { 0, 0, 0, 0 });
                        break;
                    }

                // -- 64 : Set Power Down Sequence --
                case 64:
                    {
                        timeAction("64 : Set Power Down Sequence");
                        log("Server requested setting Power Down Sequence to " + String.Join(" ", data.Select(item => item.ToString("X2"))));

                        break;
                    }

                // -- 65 : Get Power Up Timing --
                case 65:
                    {
                        timeAction("65 : Get Power Up Timing");
                        log("Server requested Power Up Timing");
                        Write(client, new byte[] { 3, 3, 3, 3 });
                        break;
                    }

                // -- 66 : Set Power Up Timing --
                case 66:
                    {
                        timeAction("66 : Set Power Up Timing");
                        log("Server requested setting Up Timing to " + String.Join(" ", data.Select(item => item.ToString("X2"))));

                        break;
                    }

                // -- 67 : Get Power Down Timing --
                case 67:
                    {
                        timeAction("67 : Get Power Down Timing");
                        log("Server requested Power Down Timing");
                        Write(client, new byte[] { 3, 6, 6, 6, 16 });

                        break;
                    }

                // -- 68 : Set Power Down Timing --
                case 68:
                    {
                        timeAction("68 : Set Power Down Timing");
                        log("Server requested setting Down Timing to " + String.Join(" ", data.Select(item => item.ToString("X2"))));

                        break;
                    }

                // ==== 10X : Test Screens ====
                // -- 101 : White --
                case 101:
                    {
                        timeAction("101 : Test White");
                        log("Server requested Test screen White");

                        screenBuffer = Enumerable.Repeat<byte>(0, screenBuffer.Length).ToArray();

                        ScreenWriteBufferBW();
                        break;
                    }

                // -- 102 : Black --
                case 102:
                    {
                        timeAction("102 : Test Black");
                        log("Server requested Test screen Black");

                        screenBuffer = Enumerable.Repeat<byte>(1, screenBuffer.Length).ToArray();

                        ScreenWriteBufferBW();

                        break;
                    }

                // -- 103 : White / Black / White --
                case 103:
                    {
                        timeAction("103 : Test White / Black / White");
                        log("Server requested Test screen Black");

                        screenBuffer = Enumerable.Repeat<byte>(0, screenBuffer.Length).ToArray();

                        ScreenWriteBufferBW();

                        screenBuffer = Enumerable.Repeat<byte>(1, screenBuffer.Length).ToArray();

                        ScreenWriteBufferBW();

                        screenBuffer = Enumerable.Repeat<byte>(0, screenBuffer.Length).ToArray();

                        ScreenWriteBufferBW();

                        break;
                    }

                // -- 104 : line --
                case 104:
                    {
                        timeAction("104 : Test Ligne");
                        log("Server requested Test screen Ligne");

                        int lineSize;

                        if (data.Count == 0)
                            lineSize = 0;
                        else
                            lineSize = data[0];

                        if (lineSize == 0)
                            lineSize = 5;

                        int offset = 0;

                        screenBuffer = Enumerable.Repeat<byte>(0, screenBuffer.Length).ToArray();

                        for (int i = 0; i < ScreenConnection.ScreenBase.Height; i++)
                        {
                            int lineCount = 0;
                            bool lineVal = false;
                            for (int j = 0; j < ScreenConnection.ScreenBase.Width; j++)
                            {
                                screenBuffer[offset++] = (byte)(lineVal ? 1 : 0);

                                lineCount++;
                                if (lineCount >= lineSize)
                                {
                                    lineCount = 0;
                                    lineVal = !lineVal;
                                }
                            }

                        }

                        ScreenWriteBufferBW();

                        break;
                    }

                // -- 105 : Col --
                case 105:
                    {
                        timeAction("105 : Test Col");
                        log("Server requested Test screen Col");

                        int colSize;

                        if (data.Count == 0)
                            colSize = 0;
                        else
                            colSize = data[0];

                        if (colSize == 0)
                            colSize = 5;

                        int offset = 0;

                        screenBuffer = Enumerable.Repeat<byte>(0, screenBuffer.Length).ToArray();

                        int colCount = 0;
                        bool colVal = false;

                        for (int i = 0; i < ScreenConnection.ScreenBase.Height; i++)
                        {
                            for (int j = 0; j < ScreenConnection.ScreenBase.Width; j++)
                            {
                                screenBuffer[offset++] = (byte)(colVal ? 1 : 0);
                            }

                            colCount++;
                            if (colCount >= colSize)
                            {
                                colCount = 0;
                                colVal = !colVal;
                            }

                        }

                        ScreenWriteBufferBW();

                        break;
                    }

                // -- 106 : square --
                case 106:
                    {
                        timeAction("106 : Test square");
                        log("Server requested Test screen square");

                        int squareSize;

                        if (data.Count == 0)
                            squareSize = 0;
                        else
                            squareSize = data[0];

                        if (squareSize == 0)
                            squareSize = 5;

                        int offset = 0;

                        screenBuffer = Enumerable.Repeat<byte>(0, screenBuffer.Length).ToArray();

                        int colCount = 0;
                        bool colVal = false;

                        for (int i = 0; i < ScreenConnection.ScreenBase.Height; i++)
                        {
                            int lineCount = 0;
                            bool lineVal = false;

                            for (int j = 0; j < ScreenConnection.ScreenBase.Width; j++)
                            {
                                screenBuffer[offset++] = (byte)(!lineVal != !colVal ? 1 : 0);

                                lineCount++;
                                if (lineCount >= squareSize)
                                {
                                    lineCount = 0;
                                    lineVal = !lineVal;
                                }
                            }

                            colCount++;
                            if (colCount >= squareSize)
                            {
                                colCount = 0;
                                colVal = !colVal;
                            }

                        }

                        ScreenWriteBufferBW();

                        break;
                    }

                // -- 107 : rand --
                case 107:
                    {
                        timeAction("107 : Test rand");
                        log("Server requested Test screen rand");

                        screenBuffer = Enumerable.Repeat<byte>(0, screenBuffer.Length).ToArray();

                        Random rnd = new Random();

                        for (int i = 0; i < screenBuffer.Length; i++)
                        {
                            screenBuffer[i] = (byte)rnd.Next(0, 2);
                        }

                        ScreenWriteBufferBW();

                        break;
                    }

                // -- 108 : scale --
                case 108:
                    {
                        timeAction("108 : Test scale");
                        log("Server requested Test screen scale");

                        screenBuffer = Enumerable.Repeat<byte>(0, screenBuffer.Length).ToArray();

                        int offset = 0;

                        for (int i = 0; i < 300; i++)
                        {
                            for (int j = 0; j < ScreenConnection.ScreenBase.Width; j++)
                            {
                                screenBuffer[offset++] = (byte)(j - 21 <= i * 2 && j > 20 ? 1 : 0);
                            }
                        }

                        for (int i = 0; i < 301; i++)
                        {
                            for (int j = 0; j < ScreenConnection.ScreenBase.Width; j++)
                            {
                                screenBuffer[offset++] = (byte)(j - 21 <= i * 2 && j > 20 ? 1 : 0);
                            }
                        }

                        ScreenWriteBufferBW();

                        break;
                    }

                // -- 109 : Grayscale Test --
                case 109:
                    {
                        timeAction("109 : Test Grayscale");
                        log("Server requested Test screen Grayscale");

                        screenBuffer = Enumerable.Repeat<byte>(0, screenBuffer.Length).ToArray();

                        int offset = 0;

                        for (int i = 0; i < ScreenConnection.ScreenBase.Height; i++)
                        {
                            int colVal = (i - 2) / (600 / 8);
                            for (int j = 0; j < ScreenConnection.ScreenBase.Width; j++)
                            {
                                screenBuffer[offset++] = (byte)colVal;
                            }
                        }

                        ScreenWriteBufferGrayScale();

                        break;
                    }

                // ==== 15X : Prints ====
                // -- 151 : Send Image Buffer --
                case 151:
                    {
                        timeAction("151 : Receive Buffer Grayscale");
                        log("Server uploaded buffer");

                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            grayScaleDepth = data[0];
                            screenBuffer = GrayScaleConverter.DecompactArray(data.Skip(1).ToArray(), grayScaleDepth, (int)(ScreenBase.Width * ScreenBase.Height));
                        }));

                        break;
                    }
                // -- 155 : Print Buffer --
                case 155:
                    {
                        timeAction("155 : Draw");
                        log("Server Started draw");

                        ScreenWriteBufferGrayScale();

                        break;
                    }
            }

            EndTransmission(client);
            log("End of processing request");
        }


        private void ScreenWriteBufferBW()
        {
            Bitmap bmp = new Bitmap(ScreenConnection.ScreenBase.Width, ScreenConnection.ScreenBase.Height);

            Color white = Color.White;
            Color black = Color.Black;

            using (var fastBitmap = bmp.FastLock())
            {
                int counter = 0;
                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        fastBitmap.SetPixel(x, y, screenBuffer[counter] == 0 ? white : black);
                        counter++;
                    }
                }
            }
            this.Dispatcher.Invoke((Action)(() =>
            {
                imgScreen.Source = BitmapToImageSource(bmp);
            }));

        }

        private void ScreenWriteBufferGrayScale()
        {
            Bitmap bmp = GrayScaleConverter.GrayToBitmap(screenBuffer, ScreenConnection.Screen.Width, ScreenConnection.Screen.Height, grayScaleDepth);

            this.Dispatcher.Invoke((Action)(() =>
            {
                imgScreen.Source = BitmapToImageSource(bmp);
            }));

        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }


        public void Write(TcpClient client, string data)
        {
            Write(client, System.Text.Encoding.ASCII.GetBytes(data));
        }

        public void Write(TcpClient client, byte[] data)
        {
            Stream stm = client.GetStream();

            List<byte> message = new List<byte>();

            message.AddRange(System.Text.Encoding.ASCII.GetBytes(packetStartToken));

            message.AddRange(BitConverter.GetBytes(data.Length));

            message.AddRange(data);

            stm.Write(message.ToArray(), 0, message.Count);
        }


        public void EndTransmission(TcpClient client)
        {
            Stream stm = client.GetStream();

            List<byte> message = new List<byte>();

            message.AddRange(System.Text.Encoding.ASCII.GetBytes(endToken));

            stm.Write(message.ToArray(), 0, message.Count);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            e.Cancel = true;
        }
    }
}
