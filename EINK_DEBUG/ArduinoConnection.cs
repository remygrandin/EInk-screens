using System;
using System.IO;
using System.IO.Ports;
using System.Linq;

namespace EINK_DEBUG
{
    public class ArduinoConnection
    {
        public ArduinoConnection(string port)
        {
            Port = port;
        }

        private static SerialPort ComPort;

        private string _port = "";
        public string Port
        {
            get { return _port; }
            set
            {
                if (ComPort != null)
                {
                    if (ComPort.IsOpen)
                        ComPort.Close();
                }

                _port = value;

                ComPort = new SerialPort(_port, 115200);
                ComPort.DtrEnable = true;
            }
        }

        public void Open()
        {
            ComPort.Open();
            StartListener();
        }

        public void Close()
        {
            ComPort.Close();
        }

        public bool IsOpen
        {
            get { return ComPort.IsOpen; }
        }

        const int OutputBufferLength = 1024000;
        private byte[] FullResult = new byte[OutputBufferLength];
        private int FullResultPos = 0;
        private int ExpectedResponseSize = 0;
        public bool CloseAfterReceive = false;

        public delegate void DataReceivedEventHandler(ArduinoResponse response);

        public event DataReceivedEventHandler DataReceived;

        private void SendOffData()
        {
            if (DataReceived != null)
            {
                byte[] outputData = new byte[FullResultPos];

                Array.Copy(FullResult, 0, outputData, 0, FullResultPos);

                FullResult = new byte[OutputBufferLength];
                FullResultPos = 0;
                ExpectedResponseSize = 0;

                DataReceived(new ArduinoResponse(outputData));
            }
        }

        public void ClearDataReceivedEvent()
        {
            DataReceived = null;
        }

        private void StartListener()
        {
            ComPort.DataReceived += ComPort_DataReceived;
        }

        private void ComPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int length = ComPort.BytesToRead;

            byte[] subBuffer = new byte[length];

            ComPort.Read(subBuffer, 0, subBuffer.Length);

            Array.Copy(subBuffer, 0, FullResult, FullResultPos, length);

            FullResultPos += length;

            if (ExpectedResponseSize == 0)
            {
                SendOffData();
            }
            else
            {
                if (FullResultPos == ExpectedResponseSize)
                {
                    if (CloseAfterReceive)
                    {
                        ComPort.DataReceived -= ComPort_DataReceived;
                        ComPort.Close();
                    }

                    SendOffData();
                }
            }
        }

        private void Send(byte actionId)
        {
            Send(actionId, new byte[0]);

        }


        private void Send(byte actionId, byte[] data)
        {
            if (!ComPort.IsOpen)
            {
                CloseAfterReceive = true;
                Open();
            }

            var message = new byte[data.Length + 5];

            message[0] = actionId; // Action Id
            Array.Copy(BitConverter.GetBytes(data.Length), 0, message, 1, 4); // Length
            Array.Copy(data, 0, message, 5, data.Length); // Data

            ComPort.Write(message, 0, message.Length);
        }

        // ==== Actions ====

        // == System ==
        // ---- Echo ----
        public void Action1Echo(byte[] data)
        {
            ExpectedResponseSize = 0;
            Send(1, data);
        }

        public void Action1Echo(string data)
        {
            Action1Echo(System.Text.Encoding.ASCII.GetBytes(data));
        }

        // == TI register getter ==
        // ---- Get All Register ----
        public void Action11GetAllRegister()
        {
            ExpectedResponseSize = 17;
            Send(11);
        }

        // ---- Get Register(s) ----
        public void Action12GetRegisters(byte address, byte size)
        {
            if(address + size > 17)
                throw new Exception("Address or size out of range");

            ExpectedResponseSize = size;
            Send(12, new byte[] { address, size });
        }

        // == TI register setter ==
        // ---- Set All Register ----
        public void Action21SetAllRegisters(byte[] values)
        {
            if(values.Length != 17)
                throw new Exception("Values array must be 17 bytes long");

            ExpectedResponseSize = 1;
            Send(21, values);
        }

        // ---- Set Register ----
        public void Action22SetRegisters(byte address, byte[] values)
        {
            if (address + values.Length > 17)
                throw new Exception("Address or size out of range");

            ExpectedResponseSize = 1;
            Send(22, new byte[] { address}.Concat(values).ToArray());
        }
        public void Action22SetRegisters(byte address, byte value)
        {
            Action22SetRegisters(address, new byte[] {value});
        }
        // == TI Functions ==
        public void Action31GetTemperature()
        {
            ExpectedResponseSize = 2;
            Send(31);
        }

        // == Screen Test Paterns ==
        public void Action41White()
        {
            ExpectedResponseSize = 0;

            Send(41);
        }

        public void Action42Black()
        {
            ExpectedResponseSize = 0;

            Send(42);
        }

        public void Action43WBWBW()
        {
            ExpectedResponseSize = 0;

            Send(43);
        }

        public void Action44Square()
        {
            ExpectedResponseSize = 0;

            Send(44);
        }

        public void Action51image(byte[] data )
        {
            ExpectedResponseSize = 0;

            Send(51, data);
        }
    }
}
