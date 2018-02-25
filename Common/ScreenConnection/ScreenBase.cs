using System;
using System.Text.RegularExpressions;

namespace ScreenConnection
{
    public class ScreenBase
    {
        private static int _width = 800;
        public static int Width => _width;
        private static int _height = 601;
        public static int Height => _height;

        private string _ip;

        public virtual string Ip
        {
            get => _ip;
            set
            {
                if (!Regex.IsMatch(value, "^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}" +
                                            "(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$"))
                    throw new ArgumentException("IP is in invalid format");
                _ip = value;
            }
        }

        private int _port;

        public virtual int Port
        {
            get => _port;
            set
            {
                if (value < 1 || value > 65535)
                    throw new ArgumentException("MAC is in invalid format");
                _port = value;
            }
        }

        private string _mac;

        public virtual string Mac
        {
            get => _mac;
            set
            {
                if (!Regex.IsMatch(value, "^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$"))
                    throw new ArgumentException("MAC is in invalid format");
                _mac = value;
            }
        }

        public override string ToString()
        {
            return _ip + ":" + _port;
        }

        public ScreenBase()
        {
        }


        public Rotation Rotation = Rotation.DEG_0;
        public int XPos = 0;
        public int YPos = 0;

        public virtual string Id { get; set; }

        public virtual void ResetId()
        {
            Id = _mac;
        }

        public virtual void Reboot()
        {
            throw new NotImplementedException();
        }

        public virtual void Shutdown()
        {
            throw new NotImplementedException();
        }

        public virtual void SendImageBuffer(byte scalegrayScaleDepth, byte[] data)
        {
            throw new NotImplementedException();
        }

        public virtual void DrawBuffer()
        {
            throw new NotImplementedException();
        }

        // ==== Power ====
        public virtual PowerStatus GetPowerStatus()
        {
            throw new NotImplementedException();
        }

        public virtual void PowerOn()
        {
            throw new NotImplementedException();
        }

        public virtual void PowerOff()
        {
            throw new NotImplementedException();
        }

        public virtual void PowerToggle()
        {
            throw new NotImplementedException();
        }

        // ==== Power Adjust ====
        public virtual int VCOM
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public virtual int VADJ
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        // ==== Temperature ====
        public virtual sbyte Temperature => throw new NotImplementedException();

        public virtual sbyte TooCold
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }


        public virtual sbyte TooHot
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        // ==== Power sequence & timing ====
        public virtual PowerSequence PowerUpSequence
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public virtual PowerSequence PowerDownSequence
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public virtual PowerUpTiming PowerUpTiming
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public virtual PowerDownTiming PowerDownTiming
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

    }
}
