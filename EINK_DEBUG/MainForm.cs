using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Windows.Forms;
using EINK_DEBUG.ActionForms;
using GrayScaleConverterLib;

namespace EINK_DEBUG
{
    public enum bits : byte
    {
        bit1 = 1,
        bit2 = 2,
        bit3 = 4,
        bit4 = 8,

        bit5 = 16,
        bit6 = 32,
        bit7 = 64,
        bit8 = 128,
    }

    public partial class MainForm : Form
    {
        public class ComboboxItem
        {
            public string Text { get; set; }
            public string Value { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RefreshComComboBoxes();
        }

        public ArduinoConnection Connection;

        // ==== Common ====
        
        private void RefreshComComboBoxes()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM WIN32_SerialPort"))
            {
                string[] portnames = SerialPort.GetPortNames();
                List<ManagementBaseObject> ports = searcher.Get().Cast<ManagementBaseObject>().ToList();
                var results = ports.Select(item => new { Id = item["DeviceID"], Name = item["Caption"] }).ToList();

                cbxCOM.Items.Clear();

                ComboboxItem firstItem = null;

                foreach (var item in results)
                {
                    ComboboxItem cbxItem = new ComboboxItem();
                    cbxItem.Text = (string)item.Name;
                    cbxItem.Value = (string)item.Id;

                    cbxCOM.Items.Add(cbxItem);

                    if (firstItem == null)
                        firstItem = cbxItem;
                }

                cbxCOM.SelectedItem = firstItem;
            }
        }

        private void btnRefreshCOM_Click(object sender, EventArgs e)
        {
            RefreshComComboBoxes();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (Connection != null && Connection.IsOpen)
            {
                Connection.Close();
            }

            Connection?.ClearDataReceivedEvent();

            Connection = new ArduinoConnection(((ComboboxItem)cbxCOM.SelectedItem).Value);
            Connection.Open();

            UnlockActions();
            btnConnect.Enabled = false;
            btnDisconnect.Enabled = true;
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            if (Connection.IsOpen)
            {
                Connection.Close();
            }

            Connection.ClearDataReceivedEvent();
            LockActions();
            btnConnect.Enabled = true;
            btnDisconnect.Enabled = false;
        }

        private void LockActions()
        {
            btnConnect.Invoke((MethodInvoker)delegate
            {
                btnReadAllRawRegisters.Enabled = false;
                btnWriteAllRawRegisters.Enabled = false;
                btnReadWriteAllRawRegisters.Enabled = false;
            });
        }

        private void UnlockActions()
        {
            btnConnect.Invoke((MethodInvoker)delegate
            {
                btnReadAllRawRegisters.Enabled = true;
                btnWriteAllRawRegisters.Enabled = true;
                btnReadWriteAllRawRegisters.Enabled = true;
            });
        }

        // ==== Raw Registers ====

        private void btnReadAllRawRegisters_Click(object sender, EventArgs e)
        {
            LockActions();

            Connection.ClearDataReceivedEvent();

            Connection.DataReceived += response =>
            {
                for (int i = 0; i < 17; i++)
                {
                    Byte currentByte = response.Data[i];
                    BitArray array = new BitArray(new byte[] {currentByte});

                    for (int j = 0; j <= 7; j++)
                    {
                        CheckBox chkbx = (CheckBox) tabRawRegister.Controls.Find("btn" + i.ToString("X2") + "h" + j, false).First();

                        var j1 = j;
                        chkbx.Invoke((MethodInvoker) delegate
                        {
                            chkbx.Checked = array.Get(j1);
                        });
                    }

                }

                UnlockActions();
            };
            Connection.Action11GetAllRegister();
        }

        private void btnWriteAllRawRegisters_Click(object sender, EventArgs e)
        {
            LockActions();

            Connection.ClearDataReceivedEvent();

            byte[] data = new byte[17];

            for (int i = 0; i < 17; i++)
            {
                BitArray array = new BitArray(8);

                for (int j = 0; j <= 7; j++)
                {
                    CheckBox chkbx = (CheckBox)tabRawRegister.Controls.Find("btn" + i.ToString("X2") + "h" + j, false).First();

                    array[j] = chkbx.Checked;
                }

                byte[] tempByte = new byte[1];
                array.CopyTo(tempByte, 0);

                data[i] = tempByte[0];
            }
            
            Connection.DataReceived += response =>
            {
                UnlockActions();
            };

            Connection.Action21SetAllRegisters(data);
        }

        private void btnReadWriteAllRawRegisters_Click(object sender, EventArgs e)
        {
            LockActions();

            Connection.ClearDataReceivedEvent();

            byte[] data = new byte[17];

            for (int i = 0; i < 17; i++)
            {
                BitArray array = new BitArray(8);

                for (int j = 0; j <= 7; j++)
                {
                    CheckBox chkbx = (CheckBox)tabRawRegister.Controls.Find("btn" + i.ToString("X2") + "h" + j, false).First();

                    array[j] = chkbx.Checked;
                }

                byte[] tempByte = new byte[1];
                array.CopyTo(tempByte, 0);

                data[i] = tempByte[0];
            }

            Connection.DataReceived += response =>
            {
                Connection.ClearDataReceivedEvent();

                Connection.DataReceived += response2 =>
                {
                    for (int i = 0; i < 17; i++)
                    {
                        Byte currentByte = response2.Data[i];
                        BitArray array = new BitArray(new byte[] { currentByte });

                        for (int j = 0; j <= 7; j++)
                        {
                            CheckBox chkbx = (CheckBox)tabRawRegister.Controls.Find("btn" + i.ToString("X2") + "h" + j, false).First();

                            var j1 = j;
                            chkbx.Invoke((MethodInvoker)delegate
                            {
                                chkbx.Checked = array.Get(j1);
                            });
                        }

                    }

                    UnlockActions();

                };

                Connection.Action11GetAllRegister();
            };

            Connection.Action21SetAllRegisters(data);
        }

        // ==== Pretty ====

        private void btnReadTemperature_Click(object sender, EventArgs e)
        {
            LockActions();

            Connection.ClearDataReceivedEvent();

            Stopwatch timer = Stopwatch.StartNew();

            Connection.DataReceived += response => 
            {
                timer.Stop();
                txtbTemperature.Invoke((MethodInvoker)delegate
                {
                    txtbTemperature.Text = response.Data[0] + " °C";
                    lblTemperatureReadTiming.Text = "Read at " + DateTime.Now.ToString("T") +
                                                    " in " + timer.ElapsedMilliseconds + " ms " +
                                                    "(" + response.Data[1] + " read)";
                });


            };

            Connection.Action31GetTemperature();
        }

        private void btnTestThroughtput_Click(object sender, EventArgs e)
        {
            /*
            Connection.ClearDataReceivedEvent();

            byte[] data = new byte[30000];

            Random rnd = new Random();

            rnd.NextBytes(data);

            Connection.DataReceived += response =>
            {

                    MessageBox.Show("Done in " + response.StringData + " µs; ", "Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            
            Connection.Action41Test();*/
        }

        // ==== Console ====

        List<Byte> consoleData = new List<byte>();

        private void RefreshConsole()
        {
            txtbStringViewConsole.Invoke((MethodInvoker)delegate
            {
                txtbStringViewConsole.Text = System.Text.Encoding.ASCII.GetString(consoleData.ToArray());
            });

            txtbHexViewConsole.Invoke((MethodInvoker)delegate
            {
                txtbHexViewConsole.Text = "";
                foreach (IEnumerable<byte> bytes in consoleData.Split(20))
                {
                    txtbHexViewConsole.Text += String.Join(" ", bytes.Select(item => item.ToString("X2"))) + "\r\n";
                }
            });
            
        }

        private Action1Echo action1Echo;

        private void btnAction1Console_Click(object sender, EventArgs e)
        {
            action1Echo?.Close();

            action1Echo = new Action1Echo(this);
            action1Echo.Show();
        }

        public void Action1EchoCall(string data)
        {
            LockActions();

            Connection.ClearDataReceivedEvent();

            Connection.DataReceived += response =>
            {
                consoleData.AddRange(response.Data);

                UnlockActions();

                RefreshConsole();
            };

            Connection.Action1Echo(data);
        }

        private void btnAction31Console_Click(object sender, EventArgs e)
        {
            Connection.ClearDataReceivedEvent();

            Connection.DataReceived += response =>
            {
                consoleData.AddRange(response.Data);

                UnlockActions();

                RefreshConsole();
            };

            Connection.Action31GetTemperature();
        }

        private void btnAction41Console_Click(object sender, EventArgs e)
        {
            /*
            Connection.ClearDataReceivedEvent();

            Connection.DataReceived += response =>
            {
                consoleData.AddRange(response.Data);

                UnlockActions();

                RefreshConsole();
            };

            byte[] data = new byte[30000];

            Random rnd = new Random();

            rnd.NextBytes(data);

            Connection.Action41Test(data);*/
        }

        private void btnTestThroughtput2_Click(object sender, EventArgs e)
        {
            /*
            Connection.ClearDataReceivedEvent();

            byte[] data = new byte[120000];

            Random rnd = new Random();

            rnd.NextBytes(data);

            Connection.DataReceived += response =>
            {
                consoleData.AddRange(response.Data);

                RefreshConsole();
                /*response.StringData
                MessageBox.Show("Done in " + response.StringData + " µs; ", "Results", MessageBoxButtons.OK, MessageBoxIcon.Information);*/
           /* };

            Connection.Action42Test(new byte[0]);*/
        }

        private void btnAction43Console_Click(object sender, EventArgs e)
        {
            Connection.ClearDataReceivedEvent();

            Connection.DataReceived += response =>
            {
                consoleData.AddRange(response.Data);

                UnlockActions();

                RefreshConsole();
            };

            Connection.Action43WBWBW();
        }

        private void btnAction44Console_Click(object sender, EventArgs e)
        {
            Connection.ClearDataReceivedEvent();

            Connection.DataReceived += response =>
            {
                consoleData.AddRange(response.Data);

                UnlockActions();

                RefreshConsole();
            };

            Connection.Action44Square();
        }

        private void btnScreenControlTestPaternWhite_Click(object sender, EventArgs e)
        {
            Connection.ClearDataReceivedEvent();

            Connection.Action41White();
        }

        private void btnScreenControlTestPaternBlack_Click(object sender, EventArgs e)
        {
            Connection.ClearDataReceivedEvent();

            Connection.Action42Black();
        }

        private void btnScreenControlTestPaternWBWBW_Click(object sender, EventArgs e)
        {
            Connection.ClearDataReceivedEvent();

            Connection.Action43WBWBW();
        }

        private void btnScreenControlTestPaternSqaures_Click(object sender, EventArgs e)
        {
            Connection.ClearDataReceivedEvent();

            Connection.Action44Square();
        }

        private void btnMLP_Click(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mlp.png"));

            byte[] grayData = GrayScaleConverter.FromBitmap(bmp, GrayScaleConverter.ConvertionMethod.Average, GrayScaleConverter.DitheringMethod.Atkinson, false, 2);

            byte[] compressedData = GrayScaleConverter.CompactArray(grayData, 2);

            Connection.Action51image(compressedData);
        }
    }
}
