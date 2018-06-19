using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace MasterControlService
{
    public class ExtScreenSerial
    {
        private Logger _logger = null;
        private ComPort _connectionPort = null;
        private SerialPort _comPort;

        public string SerilPortName => _comPort == null ? "None" : _comPort.PortName;
        public bool IsConnected = false;

        public ExtScreenSerial(Logger logger)
        {
            _logger = logger;
        }

        public class ComPort
        {
            public string Id;
            public string Name;
        }

        private List<ComPort> GetComPorts()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM WIN32_SerialPort"))
            {

                string[] portnames = SerialPort.GetPortNames();
                List<ManagementBaseObject> ports = searcher.Get().Cast<ManagementBaseObject>().ToList();
                return ports.Select(item => new ComPort() { Id = (string)item["DeviceID"], Name = (string)item["Caption"] }).ToList();
            }
        }

        public bool Locate()
        {
            List<ComPort> comPorts = GetComPorts();

            if(!comPorts.Any(item => item.Name.StartsWith("Arduino Due")))
                return false;

            ComPort port = comPorts.First(item => item.Name.StartsWith("Arduino Due"));

            _logger.Info("Found named port \"" + port.Name + "\" on port " + port.Id);

            _connectionPort = port;

            return true;
        }

        public void Connect()
        {
            _comPort = new SerialPort(_connectionPort.Id, 115200);
            _comPort.DtrEnable = true;
            _comPort.Open();
            _comPort.DataReceived += ComPort_DataReceived;
        }


        private List<byte> FullResult = new List<byte>();
        private int? ExpectedResponseSize = null;

        public delegate void DataReceivedEventHandler(string request);

        public event DataReceivedEventHandler DataReceived;

        private void ComPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int length = _comPort.BytesToRead;

            byte[] subBuffer = new byte[length];

            length = _comPort.Read(subBuffer, 0, subBuffer.Length);

            Array.Resize(ref subBuffer, length);

            FullResult.AddRange(subBuffer);
            

            if (ExpectedResponseSize == null && FullResult.Count >= 4)
            {
                byte[] sizeArray = FullResult.Take(4).ToArray();

                FullResult.RemoveRange(0,4);

                ExpectedResponseSize = BitConverter.ToInt32(sizeArray, 0);

            }

            if (FullResult.Count == ExpectedResponseSize)
            {
                string message = Encoding.ASCII.GetString(FullResult.ToArray());
                FullResult.Clear();
                ExpectedResponseSize = null;
                DataReceived(message);
            }
            
        }

        public void Send(IEnumerable<string> data)
        {
            Send(String.Join("\n", data));
        }

        public void Send(string data)
        {
            byte[] dataArray = Encoding.ASCII.GetBytes(data);

            byte[] message = new byte[dataArray.Length + 4];


            Array.Copy(BitConverter.GetBytes(dataArray.Length), 0, message, 0, 4); // Length
            Array.Copy(dataArray, 0, message, 4, data.Length); // Data

            _comPort.Write(message, 0, message.Length);
        }

    }

    
}
