using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using SerialPortLib;

namespace BandwidthTester
{
    class Program
    {
        static void Main(string[] args)
        {
            int bauds = 115200;

            Console.WriteLine("Ready to start");
            Console.ReadLine();

            testPort2("COM7", bauds);

            Console.ReadLine();
        }

        private static SerialPort arduinoBoard;

        private static Stopwatch stopwatch = new Stopwatch();

        private static void testPort(string portname, int bauds)
        {
            Console.WriteLine("Oppening port " + portname + " " + bauds + " bauds");
            arduinoBoard = new SerialPort(portname, bauds);
            arduinoBoard.DtrEnable = true;
            arduinoBoard.Open();

            byte[] buffer = new byte[4096000];

            Action kickoffRead = null;
            kickoffRead = delegate
            {

                arduinoBoard.BaseStream.BeginRead(buffer, 0, buffer.Length, delegate (IAsyncResult ar)
                {
                    try
                    {

                        int actualLength = arduinoBoard.BaseStream.EndRead(ar);

                        byte[] received = new byte[actualLength];

                        Buffer.BlockCopy(buffer, 0, received, 0, actualLength);

                        processData(received);
                    }

                    catch (IOException ex)
                    {
                        throw;

                    }
                    if (arduinoBoard.IsOpen)
                        kickoffRead();

                }, null);

            };

            kickoffRead();

            Console.WriteLine("open : " + arduinoBoard.IsOpen);

            Console.WriteLine("Sending Start");
            arduinoBoard.Write("Start");
        }

        private static void testPort2(string portname, int bauds)
        {
            Console.WriteLine("Oppening port " + portname + " " + bauds + " bauds");
            arduinoBoard = new SerialPort(portname, bauds);
            arduinoBoard.DtrEnable = true;
            arduinoBoard.Open();

            byte[] buffer = new byte[4096000];

            Action kickoffRead = null;
            kickoffRead = delegate
            {

                arduinoBoard.BaseStream.BeginRead(buffer, 0, buffer.Length, delegate (IAsyncResult ar)
                {
                    try
                    {
                        stopwatch.Stop();

                        Console.WriteLine("Done : " + stopwatch.Elapsed.ToString());
                        double os = 1000 * 92000 / stopwatch.Elapsed.TotalMilliseconds;
                        Console.WriteLine(BytesToString(Convert.ToInt64(os)) + " in 1 sec");

                        arduinoBoard.Close();
                        return;
                    }

                    catch (IOException ex)
                    {
                        throw;

                    }
                    if (arduinoBoard.IsOpen)
                        kickoffRead();

                }, null);

            };

            kickoffRead();

            Console.WriteLine("open : " + arduinoBoard.IsOpen);

            byte[] data = new byte[92000];

            Random rnd = new Random();

            rnd.NextBytes(data);


            Console.WriteLine("Sending Start");
            arduinoBoard.Write("Start2");

            Thread.Sleep(500);

            stopwatch.Start();
            arduinoBoard.BaseStream.Write(data,0, data.Length);

        }

        static List<byte> superBuffer = new List<byte>();

        static void processData(byte[] data)
        {
            superBuffer.AddRange(data);

            if (data.ToList().Contains(48))
            {
                Console.WriteLine("DataCount : " + superBuffer.Count);

                Console.WriteLine(BytesToString(superBuffer.Count) + " in 10 secs");
                Console.WriteLine(BytesToString(superBuffer.Count / 10) + " in 1 sec");

                arduinoBoard.Close();
            }
        }

        private static void ArduinoBoard_MessageReceived(object sender, MessageReceivedEventArgs args)
        {
            var a = BitConverter.ToString(args.Data);

            /*
            string datas = ((SerialPortInput) sender).ReadTo("0");

            Console.WriteLine("Checking data integrity");

            for (int i = 1; i < datas.Length; i++)
            {
                char prev = datas[i - 1];
                char cur = datas[i];

                if (!( (cur == 'a' && prev == 'z')
                    || (cur == 'b' && prev == 'a')
                    || (cur == 'c' && prev == 'b')
                    || (cur == 'd' && prev == 'c')
                    || (cur == 'e' && prev == 'd')
                    || (cur == 'f' && prev == 'e')
                    || (cur == 'g' && prev == 'f')
                    || (cur == 'h' && prev == 'g')
                    || (cur == 'i' && prev == 'h')
                    || (cur == 'j' && prev == 'i')
                    || (cur == 'k' && prev == 'j')
                    || (cur == 'l' && prev == 'k')
                    || (cur == 'm' && prev == 'l')
                    || (cur == 'n' && prev == 'm')
                    || (cur == 'o' && prev == 'n')
                    || (cur == 'p' && prev == 'o')
                    || (cur == 'q' && prev == 'p')
                    || (cur == 'r' && prev == 'q')
                    || (cur == 's' && prev == 'r')
                    || (cur == 't' && prev == 's')
                    || (cur == 'u' && prev == 't')
                    || (cur == 'v' && prev == 'u')
                    || (cur == 'w' && prev == 'v')
                    || (cur == 'x' && prev == 'w')
                    || (cur == 'y' && prev == 'x')
                    || (cur == 'z' && prev == 'y')
                    ))
                {
                    Console.WriteLine("Error at pos " + i);
                }
            }

            Console.WriteLine("DataCount : " + datas.Length);

            Console.WriteLine(BytesToString(datas.Length) + " in 3 secs");
            Console.WriteLine(BytesToString(datas.Length / 3) + " in 1 sec");

            ((SerialPort)sender).Close();

    */
        }

        static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);

            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";

            return (Math.Sign(byteCount) * num).ToString(nfi) + suf[place];
        }
    }
}
