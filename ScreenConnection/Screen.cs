using System;
using System.Net.Configuration;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;

namespace ScreenConnection
{
    public class Screen
    {
        // ==== Connections ====
        internal TcpClient TcpConnection = new TcpClient();
        internal Timer TimeoutTimer = new Timer();


        public string Ip { get; set; }

        internal Screen(string ip)
        {
            if(!Regex.IsMatch(ip, "^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\." +
                                   "(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\." +
                                   "(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\." +
                                   "(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$"))
                throw new ArgumentException("IP is in invalid format");

            Ip = ip;
            TimeoutTimer.Interval = 10_000; // 10 secs
            TimeoutTimer.Elapsed += TimeouTimer_Elapsed;
            TimeoutTimer.AutoReset = false;
        }

        private void TimeouTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            TimeoutTimer.Stop();
            TcpConnection.Close();
        }
        
        public string Mac { get; set; }

        public string Id
        {
            get => Encoding.ASCII.GetString(Connector.Action11GetId(this));
            set => Connector.Action12SetId(this, value);
        }

        public void ResetId()
        {
            Connector.Action13ResetId(this);
        }

        public void Reboot()
        {
            Connector.Action14Reboot(this);
            TcpConnection.Close();
        }

        public void Shutdown()
        {
            Connector.Action15Shutdown(this);
            TcpConnection.Close();
        }

        // ==== Power ====
        public PowerStatus GetPowerStatus()
        {
            return (PowerStatus)Connector.Action31GetPowerStatus(this)[0];
        }

        public void PowerOn()
        {
            Connector.Action32PowerOn(this);
        }

        public void PowerOff()
        {
            Connector.Action33PowerOff(this);
        }

        public void PowerToggle()
        {
            Connector.Action34PowerToggle(this);
        }

        // ==== Power Adjust ====
        public int VCOM
        {
            get => BitConverter.ToInt32(Connector.Action41GetVCOM(this), 0);
            set => Connector.Action42SetVCOM(this, value);
        }
        public int VADJ
        {
            get => BitConverter.ToInt32(Connector.Action43GetVADJ(this), 0);
            set => Connector.Action44SetVADJ(this, value);
        }

        // ==== Temperature ====
        public sbyte Temperature => (sbyte)Connector.Action51ReadTemperature(this)[0];

        public sbyte TooCold
        {
            get => (sbyte) Connector.Action52GetTooCold(this)[0];
            set => Connector.Action53SetTooCold(this, value);
        }


        public sbyte TooHot
        {
            get => (sbyte)Connector.Action54GetTooHot(this)[0];
            set => Connector.Action55SetTooHot(this, value);
        }

        // ==== Power sequence & timing ====
        public PowerSequence PowerUpSequence
        {
            get
            {
                var result = Connector.Action61GetPowerUpSequence(this);

                PowerSequence powerSeq = new PowerSequence();

                // order VPOS, VNEG, VDDH, VEE

                powerSeq.VPOS = (PowerStrobe)result[0];
                powerSeq.VNEG = (PowerStrobe)result[1];
                powerSeq.VDDH = (PowerStrobe)result[2];
                powerSeq.VEE = (PowerStrobe)result[3];

                return powerSeq;
            }
            set
            {
                // order VPOS, VNEG, VDDH, VEE
                byte[] data = new byte[4];

                data[0] = (byte)value.VPOS;
                data[1] = (byte)value.VNEG;
                data[2] = (byte)value.VDDH;
                data[3] = (byte)value.VEE;

                Connector.Action62SetPowerUpSequence(this, data);
            }
        }

        public PowerSequence PowerDownSequence
        {
            get
            {
                var result = Connector.Action63GetPowerDownSequence(this);

                PowerSequence powerSeq = new PowerSequence();

                // order VPOS, VNEG, VDDH, VEE

                powerSeq.VPOS = (PowerStrobe)result[0];
                powerSeq.VNEG = (PowerStrobe)result[1];
                powerSeq.VDDH = (PowerStrobe)result[2];
                powerSeq.VEE = (PowerStrobe)result[3];

                return powerSeq;
            }
            set
            {
                // order VPOS, VNEG, VDDH, VEE
                byte[] data = new byte[4];

                data[0] = (byte)value.VPOS;
                data[1] = (byte)value.VNEG;
                data[2] = (byte)value.VDDH;
                data[3] = (byte)value.VEE;

                Connector.Action64SetPowerDownSequence(this, data);
            }
        }

        public PowerUpTiming PowerUpTiming
        {
            get
            {
                byte[] response = Connector.Action65GetPowerUpTiming(this);

                PowerUpTiming timing = new PowerUpTiming();

                timing.StartToS1 = response[0];
                timing.S1ToS2 = response[1];
                timing.S2ToS3 = response[2];
                timing.S3ToS4 = response[3];
                return timing;
            }
            set
            {
                byte[] data = new byte[4];

                data[0] = value.StartToS1;
                data[1] = value.S1ToS2;
                data[2] = value.S2ToS3;
                data[3] = value.S3ToS4;

                Connector.Action66SetPowerUpTiming(this, data);
            }
        }

        public PowerDownTiming PowerDownTiming
        {
            get
            {
                byte[] response = Connector.Action67GetPowerDownTiming(this);

                PowerDownTiming timing = new PowerDownTiming();

                timing.StartToS1 = response[0];
                timing.S1ToS2 = response[1];
                timing.S2ToS3 = response[2];
                timing.S3ToS4 = response[3];
                timing.Multiplier = response[4];
                return timing;
            }
            set
            {
                byte[] data = new byte[5];

                data[0] = value.StartToS1;
                data[1] = value.S1ToS2;
                data[2] = value.S2ToS3;
                data[3] = value.S3ToS4;
                data[4] = value.Multiplier;

                Connector.Action68SetPowerDownTiming(this, data);
            }
        }

    }
}
