using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenConnection
{
    public class Connector
    {
        public const int ScreenPort = 2501;
        public const string DiscoveryMessage = "DISCOVERY";
        public const string EndToken = "ENDOFTRANSMISSION";
        public const string ResponsePacketStartToken = "STARTTRANSMISSION";

        public static Dictionary<string, Screen> Discovery(int msDelay = 1000)
        {
            Dictionary<string, Screen> output = new Dictionary<string, Screen>();
            try
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Parse("192.168.1.255"), ScreenPort);

                using (var udpClient = new UdpClient())
                {
                    udpClient.Client.ReceiveTimeout = msDelay;
                    udpClient.Client.ReceiveBufferSize = 64 * 1024;

                    byte[] discoveryMessageByte = Encoding.ASCII.GetBytes(DiscoveryMessage);

                    udpClient.Send(discoveryMessageByte, discoveryMessageByte.Length, ep);

                    IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, ScreenPort);

                    Thread.Sleep(msDelay);

                    byte[] receiveBuffer = udpClient.Receive(ref endpoint);

                    string receivedStr = Encoding.ASCII.GetString(receiveBuffer);

                    foreach (string response in receivedStr.Split('|').Where(item => !String.IsNullOrWhiteSpace(item)))
                    {
                        List<string> splitted = response.Split(';').ToList();

                        Screen screen = new Screen(splitted[1])
                        {
                            Id = splitted[3],
                            Ip = splitted[1],
                            Mac = splitted[2]
                        };

                        output.Add(splitted[3], screen);
                    }
                }
            }
            catch (Exception e)
            {

            }

            return output;
        }

        // ==== 0X : Diagnositcs ====
        public static byte[] Action1Ping(Screen screen)
        {
            return CallAction(screen, 1);
        }

        public static byte[] Action7DiagnosticScreen(Screen screen)
        {
            return CallAction(screen, 7);
        }

        public static byte[] Action8TestThroughput(Screen screen, byte[] data)
        {
            return CallAction(screen, 8, data);
        }

        // ==== 1X : System ====
        public static byte[] Action11GetId(Screen screen)
        {
            return CallAction(screen, 11);
        }

        public static byte[] Action12SetId(Screen screen, string id)
        {
            return CallAction(screen, 12, Encoding.ASCII.GetBytes(id));
        }

        public static byte[] Action13ResetId(Screen screen)
        {
            return CallAction(screen, 13);
        }

        public static byte[] Action14Reboot(Screen screen)
        {
            return CallAction(screen, 14);
        }

        public static byte[] Action15Shutdown(Screen screen)
        {
            return CallAction(screen, 15);
        }

        // ==== 2X : Raw Registers ====
        public static byte[] Action21GetAllRegisters(Screen screen)
        {
            return CallAction(screen, 21);
        }

        public static byte[] Action22SetAllRegisters(Screen screen, byte[] values)
        {
            if (values.Length != 17)
                throw new Exception("Invalid value length");

            return CallAction(screen, 22);
        }

        public static byte[] Action23GetRegister(Screen screen, byte address)
        {
            if (address > 17)
                throw new Exception("Invalid address");

            return CallAction(screen, 23, address );
        }

        public static byte[] Action24GetRegister(Screen screen, byte address, byte value)
        {
            if (address > 17)
                throw new Exception("Invalid address");

            return CallAction(screen, 24, new[] { address, value });
        }

        // ==== Power ====
        public static byte[] Action31GetPowerStatus(Screen screen)
        {
            return CallAction(screen, 31);
        }

        public static byte[] Action32PowerOn(Screen screen)
        {
            return CallAction(screen, 32);
        }

        public static byte[] Action33PowerOff(Screen screen)
        {
            return CallAction(screen, 33);
        }

        public static byte[] Action34PowerToggle(Screen screen)
        {
            return CallAction(screen, 34);
        }

        // ==== Power Adjust ====
        public static byte[] Action41GetVCOM(Screen screen)
        {
            return CallAction(screen, 41);
        }

        public static byte[] Action42SetVCOM(Screen screen, int value)
        {
            if(value % 10 != 0 || value > 0 || value <= -5110 )
                throw new ArgumentException("Value must be a multiple of 10 between -5110 and 0");

            return CallAction(screen, 42, BitConverter.GetBytes(value));
        }

        public static byte[] Action43GetVADJ(Screen screen)
        {
            return CallAction(screen, 43);
        }

        public static byte[] Action44SetVADJ(Screen screen, int value)
        {
            if (value != 15000 && value != 14750 && value != 14500 && value != 14250)
                throw new ArgumentException("Value must be a either 15000 or 14750 or 14500 or 14250");

            return CallAction(screen, 44, BitConverter.GetBytes(value));
        }


        // ==== Temperature ====
        public static byte[] Action51ReadTemperature(Screen screen)
        {
            return CallAction(screen, 51);
        }

        public static byte[] Action52GetTooCold(Screen screen)
        {
            return CallAction(screen, 52);
        }

        public static byte[] Action53SetTooCold(Screen screen, sbyte value)
        {
            if (value < -7 || value > 8)
                throw new ArgumentException("Value must be between -7 and 8");

            return CallAction(screen, 53, (byte)value);
        }

        public static byte[] Action54GetTooHot(Screen screen)
        {
            return CallAction(screen, 54);
        }

        public static byte[] Action55SetTooHot(Screen screen, sbyte value)
        {
            if (value < 42 || value > 57)
                throw new ArgumentException("Value must be between 42 and 57");

            return CallAction(screen, 55,  (byte)value);
        }

        // ==== Power Up/Down sequence and timing ====
        public static byte[] Action61GetPowerUpSequence(Screen screen)
        {
            return CallAction(screen, 61);
        }

        public static byte[] Action62SetPowerUpSequence(Screen screen, byte[] data)
        {
            return CallAction(screen, 62, data);
        }

        public static byte[] Action63GetPowerDownSequence(Screen screen)
        {
            return CallAction(screen, 63);
        }

        public static byte[] Action64SetPowerDownSequence(Screen screen, byte[] data)
        {
            return CallAction(screen, 64, data);
        }

        public static byte[] Action65GetPowerUpTiming(Screen screen)
        {
            return CallAction(screen, 65);
        }

        public static byte[] Action66SetPowerUpTiming(Screen screen, byte[] data)
        {
            return CallAction(screen, 66, data);
        }

        public static byte[] Action67GetPowerDownTiming(Screen screen)
        {
            return CallAction(screen, 67);
        }

        public static byte[] Action68SetPowerDownTiming(Screen screen, byte[] data)
        {
            return CallAction(screen, 68, data);
        }

        // ==== Test Screens ====
        public static byte[] Action101TestWhite(Screen screen)
        {
            return CallAction(screen, 101);
        }

        public static byte[] Action102TestBlack(Screen screen)
        {
            return CallAction(screen, 102);
        }

        public static byte[] Action103TestWbw(Screen screen)
        {
            return CallAction(screen, 103);
        }

        public static byte[] Action104TestLines(Screen screen, byte size)
        {
            return CallAction(screen, 104, size);
        }

        public static byte[] Action105TestColumns(Screen screen, byte size)
        {
            return CallAction(screen, 105, size);
        }

        public static byte[] Action106TestSquares(Screen screen, byte size)
        {
            return CallAction(screen, 106, size);
        }

        public static byte[] Action107TestRand(Screen screen)
        {
            return CallAction(screen, 107);
        }

        public static byte[] Action108TestScale(Screen screen)
        {
            return CallAction(screen, 108);
        }

        public static byte[] Action109TestGrayScale(Screen screen, byte scales)
        {
            return CallAction(screen, 109, scales);
        }

        public static byte[] Action151PrintGrey(Screen screen, byte[] data)
        {
            return CallAction(screen, 151, data);
        }







        private static byte[] CallAction(Screen screen, byte actionId)
        {
            return CallAction(screen, actionId, new byte[0]);
        }

        private static byte[] CallAction(Screen screen, byte actionId, byte data)
        {
            return CallAction(screen, actionId, new[] { data });
        }

        private static byte[] CallAction(Screen screen, byte actionId, byte[] data)
        {
            if (data == null)
                data = new byte[0];



            //screen.TimeoutTimer.Stop();
            //screen.TimeoutTimer.Start();

            if (!screen.TcpConnection.Connected)
            {
                screen.TcpConnection = new TcpClient();
                screen.TcpConnection.Connect(screen.Ip, ScreenPort);
            }

            Stream stm = screen.TcpConnection.GetStream();

            // Request Datagram Format
            // +----------------------+
            // |  Action ID (1 Byte)  |
            // +----------------------+
            // |      Data length     |
            // |         Int32        |
            // |       BigEndian      |
            // |       (4 Bytes)      |
            // +----------------------+
            // |          ...         |
            // |          Data        |
            // |          ...         |
            // +----------------------+
            // |       End Token      |
            // +----------------------+

            // Response Datagram Format
            // +----------------------+
            // |      Data length     |
            // |         Int32        |
            // |       BigEndian      |
            // |       (4 Bytes)      |
            // +----------------------+
            // |          ...         |
            // |          Data        |
            // |          ...         |
            // +----------------------+
            // |      Data length     |  // 
            // |         Int32        |  //
            // |       BigEndian      |  //
            // |       (4 Bytes)      |  //  <== Optional repeat
            // +----------------------+  //
            // |          ...         |  //
            // |          Data        |  //
            // |          ...         |  //
            // +----------------------+
            // |       End Token      |
            // +----------------------+

            // Building request
            var message = new byte[data.Length + 5 + EndToken.Length];

            message[0] = (byte)actionId; // Action Id
            Array.Copy(BitConverter.GetBytes(data.Length), 0, message, 1, 4); // Length
            Array.Copy(data, 0, message, 5, data.Length); // Data
            Array.Copy(Encoding.ASCII.GetBytes(EndToken), 0, message, data.Length + 5, EndToken.Length);


            stm.Write(message, 0, message.Length);

            int subBufferSize = 1024;

            ReceiveState state = ReceiveState.AwaitingHeader;
            int expectedSize = -1;

            List<byte> response = new List<byte>();

            List<byte> packetBuffer = new List<byte>();

            while (true)
            {
                byte[] subBuffer = new byte[subBufferSize];

                int read = stm.Read(subBuffer, 0, subBufferSize);

                if (read != 0)
                {
                    Array.Resize(ref subBuffer, read);

                    packetBuffer.AddRange(subBuffer);

                    bool continueReading = false;

                    while (!continueReading)
                    {
                        switch (state)
                        {
                            case ReceiveState.AwaitingHeader:
                                if (Encoding.ASCII.GetString(packetBuffer.Take(ResponsePacketStartToken.Length).ToArray()) == ResponsePacketStartToken)
                                {
                                    state = ReceiveState.AwaitingSize;
                                    packetBuffer.RemoveRange(0, ResponsePacketStartToken.Length);
                                }
                                else if (Encoding.ASCII.GetString(packetBuffer.Take(EndToken.Length).ToArray()) == EndToken)
                                {
                                    state = ReceiveState.Exiting;
                                }
                                else
                                {
                                    continueReading = true;
                                }
                                break;
                            case ReceiveState.AwaitingSize:

                                if (packetBuffer.Count < 4)
                                {
                                    continueReading = true;
                                }
                                else
                                {
                                    byte[] size = packetBuffer.Take(4).ToArray();

                                    expectedSize = BitConverter.ToInt32(size, 0);

                                    packetBuffer.RemoveRange(0, 4);

                                    state = ReceiveState.AwaitingData;
                                }
                                break;
                            case ReceiveState.AwaitingData:
                                if (packetBuffer.Count < expectedSize)
                                {
                                    continueReading = true;
                                }
                                else
                                {
                                    response.AddRange(packetBuffer.Take(expectedSize));

                                    packetBuffer.RemoveRange(0, expectedSize);

                                    state = ReceiveState.AwaitingHeader;
                                }
                                break;
                            case ReceiveState.Exiting:
                                continueReading = true;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    if (state == ReceiveState.Exiting)
                        break;

                }
                if (read == 0)
                    Thread.Sleep(10);

            }

            return response.ToArray();
        }

        private enum ReceiveState
        {
            AwaitingHeader,
            AwaitingSize,
            AwaitingData,
            Exiting
        }
    }
}
