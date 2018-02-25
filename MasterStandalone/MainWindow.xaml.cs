using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using ScreenConnection;
using Path = System.IO.Path;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GrayScaleConverterLib;
using Microsoft.Win32;
using Rectangle = System.Drawing.Rectangle;
using MasterModuleCommon;
using Point = System.Drawing.Point;
using Rotation = ScreenConnection.Rotation;
using Size = System.Drawing.Size;

namespace MasterStandalone
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Finds a Child of a given item in the visual tree. 
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, 
        /// a null parent is being returned.</returns>
        public static T FindChild<T>(DependencyObject parent, string childName)
            where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        private BitmapImage bitmapToImageSource(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        public Dictionary<string, List<Tuple<string, bool>>> registers = new Dictionary<string, List<Tuple<string, bool>>>()
        {
            {"00_TMST_VALUE",new List<Tuple<string, bool>>
            {
                Tuple.Create("TMST_VALUE[0]", false), Tuple.Create("TMST_VALUE[1]", false),
                Tuple.Create("TMST_VALUE[2]", false), Tuple.Create("TMST_VALUE[3]", false),
                Tuple.Create("TMST_VALUE[4]", false), Tuple.Create("TMST_VALUE[5]", false),
                Tuple.Create("TMST_VALUE[6]", false), Tuple.Create("TMST_VALUE[7]", false)
            }},
            {"01_ENABLE",new List<Tuple<string, bool>>
            {
                Tuple.Create("VNEG_EN", true), Tuple.Create("VEE_EN", true),
                Tuple.Create("VPOS_EN", true), Tuple.Create("VDDH_EN", true),
                Tuple.Create("VCOM_EN", true), Tuple.Create("V3P3_EN", true),
                Tuple.Create("STANDBY", true), Tuple.Create("ACTIVE", true)
            }},
            {"02_VADJ",new List<Tuple<string, bool>>
            {
                Tuple.Create("VSET[0]", true), Tuple.Create("VSET[1]", true),
                Tuple.Create("VSET[2]", true), Tuple.Create("", false),
                Tuple.Create("", false), Tuple.Create("", false),
                Tuple.Create("", false), Tuple.Create("", false)
            }},
            {"03_VCOM",new List<Tuple<string, bool>>
            {
                Tuple.Create("VCOM[0]", true), Tuple.Create("VCOM[1]", true),
                Tuple.Create("VCOM[2]", true), Tuple.Create("VCOM[3]", true),
                Tuple.Create("VCOM[4]", true), Tuple.Create("VCOM[5]", true),
                Tuple.Create("VCOM[6]", true), Tuple.Create("VCOM[7]", true)
            }},
            {"04_VCOM2",new List<Tuple<string, bool>>
            {
                Tuple.Create("VCOM[8]", true), Tuple.Create("", false),
                Tuple.Create("", false), Tuple.Create("AVG[0]", true),
                Tuple.Create("AVG[1]", true), Tuple.Create("HiZ", true),
                Tuple.Create("PROG", true), Tuple.Create("ACQ", true)
            }},
            {"05_INT_EN1",new List<Tuple<string, bool>>
            {
                Tuple.Create("PRGC_EN", false), Tuple.Create("ACQC_EN", false),
                Tuple.Create("UVLO_EN", true), Tuple.Create("TMST_COLD_EN", true),
                Tuple.Create("TMST_HOT_EN", true), Tuple.Create("HOT_EN", true),
                Tuple.Create("TSD_EN", true), Tuple.Create("DTX_EN", false)
            }},
            {"06_INT_EN2",new List<Tuple<string, bool>>
            {
                Tuple.Create("EOCEN", true), Tuple.Create("VNEGUVEN", true),
                Tuple.Create("VCOMFEN", true), Tuple.Create("VEEUVEN", true),
                Tuple.Create("VPOSUVEN", true), Tuple.Create("VNUV_EN", true),
                Tuple.Create("VDDHUVEN", true), Tuple.Create("VBUVEN", true)
            }},
            {"07_INT1",new List<Tuple<string, bool>>
            {
                Tuple.Create("PRGC", false), Tuple.Create("ACQC", false),
                Tuple.Create("UVLO", false), Tuple.Create("TMST_COLD", false),
                Tuple.Create("TMST_HOT", false), Tuple.Create("HOT", false),
                Tuple.Create("TSD", false), Tuple.Create("DTX", false)
            }},
            {"08_INT2",new List<Tuple<string, bool>>
            {
                Tuple.Create("EOC", false), Tuple.Create("VNEG_UV", false),
                Tuple.Create("VCOMF", false), Tuple.Create("VEE_UV", false),
                Tuple.Create("VPOS_UV", false), Tuple.Create("VN_UV", false),
                Tuple.Create("VDDH_UV", false), Tuple.Create("VB_UV", false)
            }},
            {"09_UPSEQ0",new List<Tuple<string, bool>>
            {
                Tuple.Create("VNEG_UP[0]", true), Tuple.Create("VNEG_UP[1]", true),
                Tuple.Create("VEE_UP[0]", true), Tuple.Create("VEE_UP[1]", true),
                Tuple.Create("VPOS_UP[0]", true), Tuple.Create("VPOS_UP[1]", true),
                Tuple.Create("VDDH_UP[0]", true), Tuple.Create("VDDH_UP[1]", true)
            }},
            {"0A_UPSEQ1",new List<Tuple<string, bool>>
            {
                Tuple.Create("UDLY1[0]", true), Tuple.Create("UDLY1[1]", true),
                Tuple.Create("UDLY2[0]", true), Tuple.Create("UDLY2[1]", true),
                Tuple.Create("UDLY3[0]", true), Tuple.Create("UDLY3[1]", true),
                Tuple.Create("UDLY4[0]", true), Tuple.Create("UDLY4[1]", true)
            }},
            {"0B_DOWNSEQ0",new List<Tuple<string, bool>>
            {
                Tuple.Create("VNEG_DWN[0]", true), Tuple.Create("VNEG_DWN[1]", true),
                Tuple.Create("VEE_DWN[0]", true), Tuple.Create("VEE_DWN[1]", true),
                Tuple.Create("VPOS_DWN[0]", true), Tuple.Create("VPOS_DWN[1]", true),
                Tuple.Create("VDDH_DWN[0]", true), Tuple.Create("VDDH_DWN[1]", true)
            }},
            {"0C_DOWNSEQ1",new List<Tuple<string, bool>>
            {
                Tuple.Create("DFCTR", true), Tuple.Create("DDLY1", true),
                Tuple.Create("DDLY2[0]", true), Tuple.Create("DDLY2[1]", true),
                Tuple.Create("DDLY3[0]", true), Tuple.Create("DDLY3[1]", true),
                Tuple.Create("DDLY4[0]", true), Tuple.Create("DDLY4[1]", true)
            }},
            {"0D_TMST1",new List<Tuple<string, bool>>
            {
                Tuple.Create("DT[0]", true), Tuple.Create("DT[1]", true),
                Tuple.Create("", false), Tuple.Create("", false),
                Tuple.Create("", false), Tuple.Create("CONV_END", false),
                Tuple.Create("", false), Tuple.Create("READ_THERM", true)
            }},
            {"0E_TMST2",new List<Tuple<string, bool>>
            {
                Tuple.Create("TMST_HOT[0]", true), Tuple.Create("TMST_HOT[1]", true),
                Tuple.Create("TMST_HOT[2]", true), Tuple.Create("TMST_HOT[3]", true),
                Tuple.Create("TMST_COLD[0]", true), Tuple.Create("TMST_COLD[1]", true),
                Tuple.Create("TMST_COLD[2]", true), Tuple.Create("TMST_COLD[3]", true)
            }},
            {"0F_PG",new List<Tuple<string, bool>>
            {
                Tuple.Create("", false), Tuple.Create("VNEG_PG", false),
                Tuple.Create("", false), Tuple.Create("VEE_PG", false),
                Tuple.Create("VPOS_PG", false), Tuple.Create("VN_PG", false),
                Tuple.Create("VDDH_PG", false), Tuple.Create("VB_PG", false)
            }},
            {"10_REVID",new List<Tuple<string, bool>>
            {
                Tuple.Create("VERSION[0]", false), Tuple.Create("VERSION[1]", false),
                Tuple.Create("VERSION[2]", false), Tuple.Create("VERSION[3]", false),
                Tuple.Create("MNREV[0]", false), Tuple.Create("MNREV[1]", false),
                Tuple.Create("MJREV[0]", false), Tuple.Create("MJREV[1]", false)
            }}
        };


        public MainWindow()
        {
            InitializeComponent();
            refreshScreenList();

            generateRegistersScreen();

        }

        private void generateRegistersScreen()
        {
            int yPos = 50;
            int xStartPos = 900;

            Style style = this.FindResource("disabledAndCheckedButton") as Style;

            foreach (string key in registers.Keys.OrderBy(item => item))
            {
                string register = key.Split('_').First();
                List<Tuple<string, bool>> line = registers[key];

                int xPos = xStartPos;

                Button btnWrite = new Button();

                btnWrite.Name = "btnRegisterR" + register + "Write";
                btnWrite.Content = "Write";
                btnWrite.VerticalAlignment = VerticalAlignment.Top;
                btnWrite.HorizontalAlignment = HorizontalAlignment.Left;
                btnWrite.Height = 25;
                btnWrite.Width = 50;

                btnWrite.Margin = new Thickness(xPos, yPos, 0, 0);

                btnWrite.Click += BtnWrite_Click;

                registerGrid.Children.Add(btnWrite);

                xPos -= 55;

                Button btnRead = new Button();

                btnRead.Name = "btnRegisterR" + register + "Read";
                btnRead.Content = "Read";
                btnRead.VerticalAlignment = VerticalAlignment.Top;
                btnRead.HorizontalAlignment = HorizontalAlignment.Left;
                btnRead.Height = 25;
                btnRead.Width = 50;

                btnRead.Margin = new Thickness(xPos, yPos, 0, 0);

                btnRead.Click += BtnRead_Click;

                registerGrid.Children.Add(btnRead);

                xPos -= 92;


                int pos = 0;
                foreach (Tuple<string, bool> tuple in line)
                {
                    ToggleButton tb = new ToggleButton();

                    tb.Name = "btnRegisterR" + register + "P" + pos;
                    tb.Content = tuple.Item1;
                    tb.IsEnabled = tuple.Item2;
                    tb.VerticalAlignment = VerticalAlignment.Top;
                    tb.HorizontalAlignment = HorizontalAlignment.Left;
                    tb.Height = 25;
                    tb.Width = 87;
                    tb.Style = style;



                    tb.Margin = new Thickness(xPos, yPos, 0, 0);
                    xPos -= 92;
                    pos++;

                    registerGrid.Children.Add(tb);
                }

                xPos -= 10;

                TextBlock txtb = new TextBlock();

                txtb.Text = key;
                txtb.VerticalAlignment = VerticalAlignment.Top;
                txtb.HorizontalAlignment = HorizontalAlignment.Left;

                txtb.Margin = new Thickness(xPos, yPos + 4, 0, 0);
                registerGrid.Children.Add(txtb);

                yPos += 30;
            }
        }

        private void BtnRead_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            string register = btn.Name.Substring(12, 2);

            byte registerParsed = byte.Parse(register, NumberStyles.HexNumber);

            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            byte[] data = Connector.Action23GetRegister(screen, registerParsed);

            BitArray ba = new BitArray(data);

            for (int pos = 0; pos <= 7; pos++)
            {
                ToggleButton btnRegister = FindChild<ToggleButton>(this, "btnRegisterR" + register + "P" + pos);

                btnRegister.IsChecked = ba[pos];
            }

        }

        private void BtnWrite_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            string register = btn.Name.Substring(12, 2);

            byte registerParsed = byte.Parse(register, NumberStyles.HexNumber);

            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            BitArray ba = new BitArray(new byte[] { 0 });

            for (int pos = 0; pos <= 7; pos++)
            {
                ToggleButton btnRegister = FindChild<ToggleButton>(this, "btnRegisterR" + register + "P" + pos);

                ba[pos] = btnRegister.IsChecked ?? false;
            }

            byte[] bytes = new byte[1];
            ba.CopyTo(bytes, 0);

            Connector.Action24GetRegister(screen, registerParsed, bytes[0]);
        }

        private async void BtnEcho_OnClick(object sender, RoutedEventArgs e)
        {
            Bitmap bmp = new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mlp.jpg"));

            Bitmap newImage = new Bitmap(800, 601);

            using (Graphics gr = Graphics.FromImage(newImage))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.DrawImage(bmp, new Rectangle(0, 0, 800, 601));

                gr.Save();
            }

            byte[] grayData = GrayScaleConverter.FromBitmap(newImage, GrayScaleConverter.ConvertionMethod.Desaturation, GrayScaleConverter.DitheringMethod.Atkinson, false, 8);

            grayData = GrayScaleConverter.ReverseGrayScale(grayData, 8);

            //await Connector.testImage("", grayData);

        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            refreshScreenList();
        }

        private void refreshScreenList()
        {
            Dictionary<string, Screen> screens = Connector.Discovery();

            cmbxScreenList.ItemsSource = screens.Select(item => item.Value).ToList();

            cmbxScreenList.SelectedItem = screens.FirstOrDefault().Value;

        }

        private void inpScreenList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void btnReadAll_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            byte[] data = Connector.Action21GetAllRegisters(screen);

            for (int registerNb = 0; registerNb < 17; registerNb++)
            {
                string register = registerNb.ToString("X").PadLeft(2, '0');
                BitArray b = new BitArray(new byte[] { data[registerNb] });

                for (int pos = 0; pos <= 7; pos++)
                {
                    ToggleButton btn = FindChild<ToggleButton>(this, "btnRegisterR" + register + "P" + pos);

                    btn.IsChecked = b[pos];
                }
            }
        }

        private void btnWriteAll_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            byte[] data = new byte[17];

            for (int registerNb = 0; registerNb < 17; registerNb++)
            {
                string register = registerNb.ToString("X").PadLeft(2, '0');
                BitArray ba = new BitArray(new byte[] { 0 });

                for (int pos = 0; pos <= 7; pos++)
                {
                    ToggleButton btn = FindChild<ToggleButton>(this, "btnRegisterR" + register + "P" + pos);

                    ba[pos] = btn.IsChecked ?? false;
                }

                ba.CopyTo(data, registerNb);
            }

            Connector.Action22SetAllRegisters(screen, data);
        }

        private void btnDiagnostic_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;
            Connector.Action7DiagnosticScreen(screen);
        }

        private void btnTestWhite_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;
            Connector.Action101TestWhite(screen);
        }

        private void btnTestBlack_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;
            Connector.Action102TestBlack(screen);
        }

        private void btnTestWBW_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;
            Connector.Action103TestWbw(screen);
        }

        private void btnTestLines_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            byte count = byte.Parse(inpTestLines.Text);

            Connector.Action104TestLines(screen, count);
        }

        private void btnTestColumns_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            byte count = byte.Parse(inpTestColumns.Text);

            Connector.Action105TestColumns(screen, count);
        }

        private void btnTestSquares_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            byte count = byte.Parse(inpTestSquares.Text);

            Connector.Action106TestSquares(screen, count);
        }

        private void btnTestRand_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;
            Connector.Action107TestRand(screen);
        }

        private void btnTestScale_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;
            Connector.Action108TestScale(screen);
        }

        private void btnTestGrayScale_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            byte count = byte.Parse(inpTestGrayScales.Text);

            Connector.Action109TestGrayScale(screen, count);
        }

        private void loadImage(string path)
        {
            //Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mlp.png")
            Bitmap bmp = new Bitmap(path);

            Bitmap bmpResized = new Bitmap(bmp, 800, 601);


            int depth = 8;

            GrayScaleConverter.ConvertionMethod method = GrayScaleConverter.ConvertionMethod.DecompositionMax;
            GrayScaleConverter.DitheringMethod dithering = GrayScaleConverter.DitheringMethod.Atkinson;
            bool serpentine = false;


            grayData = GrayScaleConverter.FromBitmap(bmpResized, method, dithering, serpentine, depth);

            Bitmap grayBmp = GrayScaleConverter.GrayToBitmap(grayData, bmpResized.Width, bmpResized.Height, depth);

            grayData = GrayScaleConverter.ReverseGrayScale(grayData, depth);

            imgPreview.Source = bitmapToImageSource(grayBmp);
        }

        private byte[] grayData;

        private void btnPreviewBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (ofd.ShowDialog() == true)
            {
                txtbPreviewPath.Text = ofd.FileName;
                loadImage(txtbPreviewPath.Text);

            }



        }

        private void btnPrintPreview_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            Bitmap bmp = new Bitmap(txtbPreviewPath.Text);

            Point[] points = GraphicHelper.ComputeTargetPoints(new Size(800, 601), new Size(bmp.Width, bmp.Height), Rotation.DEG_0);

            Bitmap bmpResized = new Bitmap(800,601);

            using (Graphics gr = Graphics.FromImage(bmpResized))
            {
                gr.DrawImage(bmp, points);
            }

            grayData = GrayScaleConverter.ConvertToGrayscale(bmpResized, GrayScaleConverter.ConvertionMethod.AverageBT601, 8);

            grayData = GrayScaleConverter.DitherSierraLight(grayData, 8, bmpResized.Width, bmpResized.Height);

            grayData = GrayScaleConverter.ReverseGrayScale(grayData, 8);

            grayData = GrayScaleConverter.CompactArray(grayData, 8);

            screen.SendImageBuffer(8, grayData);

            screen.DrawBuffer();
        }

        private void btnPowerRefresh_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            PowerStatus status = screen.GetPowerStatus();

            SolidColorBrush red = new SolidColorBrush(Colors.Red);
            SolidColorBrush green = new SolidColorBrush(Colors.Green);
            SolidColorBrush yellow = new SolidColorBrush(Colors.Yellow);
            SolidColorBrush gray = new SolidColorBrush(Colors.Gray);

            if (status.HasFlag(PowerStatus.SOURCE_ON))
            {
                indPowerSource.Fill = green;
            }
            else if (status.HasFlag(PowerStatus.VPOS_PG) || status.HasFlag(PowerStatus.VNEG_PG))
            {
                indPowerSource.Fill = yellow;
            }
            else
            {
                indPowerSource.Fill = red;
            }

            if (status.HasFlag(PowerStatus.GATE_ON))
            {
                indPowerGate.Fill = green;
            }
            else if (status.HasFlag(PowerStatus.VDDH_PG) || status.HasFlag(PowerStatus.VEE_PG))
            {
                indPowerGate.Fill = yellow;
            }
            else
            {
                indPowerGate.Fill = red;
            }


            if (status.HasFlag(PowerStatus.V3P3_PG))
            {
                indPowerV3P3.Fill = green;
            }
            else
            {
                indPowerV3P3.Fill = red;
            }
        }

        private void btnPowerON_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;
            screen.PowerOn();
            Thread.Sleep(100);
            btnPowerRefresh_Click(null, null);
        }

        private void btnPowerOFF_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;
            screen.PowerOff();
            Thread.Sleep(100);
            btnPowerRefresh_Click(null, null);
        }

        private void btnPowerToggle_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;
            screen.PowerToggle();
            Thread.Sleep(100);
            btnPowerRefresh_Click(null, null);
        }

        private void btnReadTemperature_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            lblTemperature.Content = "Reading : -°C".Replace("-", screen.Temperature.ToString());
        }

        private void btnReadTooCold_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            string value = screen.TooCold.ToString();

            foreach (ListBoxItem item in cmbxTooCold.Items)
            {
                if (item.Uid == value)
                    cmbxTooCold.SelectedItem = item;
            }
        }

        private void btnWriteTooCold_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            screen.TooCold = sbyte.Parse(((ListBoxItem)cmbxTooCold.SelectedItem).Uid);
        }

        private void btnReadTooHot_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            string value = screen.TooHot.ToString();

            foreach (ListBoxItem item in cmbxTooHot.Items)
            {
                if (item.Uid == value)
                    cmbxTooHot.SelectedItem = item;
            }
        }

        private void btnWriteTooHot_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            screen.TooHot = sbyte.Parse(((ListBoxItem)cmbxTooHot.SelectedItem).Uid);
        }

        private void btnReadId_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            txtbScreenId.Text = screen.Id;
        }

        private void btnWriteId_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            screen.Id = txtbScreenId.Text;
        }

        private void btnResetId_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            screen.ResetId();

            btnReadId_Click(null, null);
        }

        private void btnReboot_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            screen.Reboot();
        }

        private void btnShutdown_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            screen.Shutdown();
        }

        private void btnReadVCOM_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            string value = screen.VCOM.ToString();

            foreach (ListBoxItem item in cmbxVCOM.Items)
            {
                if (item.Uid == value)
                    cmbxVCOM.SelectedItem = item;
            }
        }

        private void btnWriteVCOM_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            screen.VCOM = int.Parse(((ListBoxItem)cmbxVCOM.SelectedItem).Uid);
        }



        private void btnReadVADJ_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            string value = screen.VADJ.ToString();

            foreach (ListBoxItem item in cmbxVADJ.Items)
            {
                if (item.Uid == value)
                    cmbxVADJ.SelectedItem = item;
            }
        }

        private void btnWriteVADJ_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            screen.VADJ = int.Parse(((ListBoxItem)cmbxVADJ.SelectedItem).Uid);
        }




        private void btnReadPowerUpSeq_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            PowerSequence seq = screen.PowerUpSequence;

            foreach (ListBoxItem item in cmbxPowerUpSeqVPOS.Items)
            {
                if (item.Uid == ((byte)seq.VPOS).ToString())
                    cmbxPowerUpSeqVPOS.SelectedItem = item;
            }

            foreach (ListBoxItem item in cmbxPowerUpSeqVNEG.Items)
            {
                if (item.Uid == ((byte)seq.VNEG).ToString())
                    cmbxPowerUpSeqVNEG.SelectedItem = item;
            }

            foreach (ListBoxItem item in cmbxPowerUpSeqVDDH.Items)
            {
                if (item.Uid == ((byte)seq.VDDH).ToString())
                    cmbxPowerUpSeqVDDH.SelectedItem = item;
            }

            foreach (ListBoxItem item in cmbxPowerUpSeqVEE.Items)
            {
                if (item.Uid == ((byte)seq.VEE).ToString())
                    cmbxPowerUpSeqVEE.SelectedItem = item;
            }

        }

        private void btnWritePowerUpSeq_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            PowerSequence seq = screen.PowerUpSequence;

            seq.VPOS = (PowerStrobe)byte.Parse(((ListBoxItem)cmbxPowerUpSeqVPOS.SelectedItem).Uid);
            seq.VNEG = (PowerStrobe)byte.Parse(((ListBoxItem)cmbxPowerUpSeqVNEG.SelectedItem).Uid);
            seq.VDDH = (PowerStrobe)byte.Parse(((ListBoxItem)cmbxPowerUpSeqVDDH.SelectedItem).Uid);
            seq.VEE = (PowerStrobe)byte.Parse(((ListBoxItem)cmbxPowerUpSeqVEE.SelectedItem).Uid);

            screen.PowerUpSequence = seq;
        }

        private void btnReadPowerDownSeq_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            PowerSequence seq = screen.PowerDownSequence;

            foreach (ListBoxItem item in cmbxPowerDownSeqVPOS.Items)
            {
                if (item.Uid == ((byte)seq.VPOS).ToString())
                    cmbxPowerDownSeqVPOS.SelectedItem = item;
            }

            foreach (ListBoxItem item in cmbxPowerDownSeqVNEG.Items)
            {
                if (item.Uid == ((byte)seq.VNEG).ToString())
                    cmbxPowerDownSeqVNEG.SelectedItem = item;
            }

            foreach (ListBoxItem item in cmbxPowerDownSeqVDDH.Items)
            {
                if (item.Uid == ((byte)seq.VDDH).ToString())
                    cmbxPowerDownSeqVDDH.SelectedItem = item;
            }

            foreach (ListBoxItem item in cmbxPowerDownSeqVEE.Items)
            {
                if (item.Uid == ((byte)seq.VEE).ToString())
                    cmbxPowerDownSeqVEE.SelectedItem = item;
            }
        }

        private void btnWritePowerDownSeq_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            PowerSequence seq = new PowerSequence();

            seq.VPOS = (PowerStrobe)byte.Parse(((ListBoxItem)cmbxPowerDownSeqVPOS.SelectedItem).Uid);
            seq.VNEG = (PowerStrobe)byte.Parse(((ListBoxItem)cmbxPowerDownSeqVNEG.SelectedItem).Uid);
            seq.VDDH = (PowerStrobe)byte.Parse(((ListBoxItem)cmbxPowerDownSeqVDDH.SelectedItem).Uid);
            seq.VEE = (PowerStrobe)byte.Parse(((ListBoxItem)cmbxPowerDownSeqVEE.SelectedItem).Uid);

            screen.PowerDownSequence = seq;
        }

        private void btnReadPowerUpTiming_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            PowerUpTiming timing = screen.PowerUpTiming;

            foreach (ListBoxItem item in cmbxPowerUpTimingStToS1.Items)
            {
                if (item.Uid == ((byte)timing.StartToS1).ToString())
                    cmbxPowerUpTimingStToS1.SelectedItem = item;
            }

            foreach (ListBoxItem item in cmbxPowerUpTimingS1ToS2.Items)
            {
                if (item.Uid == ((byte)timing.S1ToS2).ToString())
                    cmbxPowerUpTimingS1ToS2.SelectedItem = item;
            }

            foreach (ListBoxItem item in cmbxPowerUpTimingS2ToS3.Items)
            {
                if (item.Uid == ((byte)timing.S2ToS3).ToString())
                    cmbxPowerUpTimingS2ToS3.SelectedItem = item;
            }

            foreach (ListBoxItem item in cmbxPowerUpTimingS3ToS4.Items)
            {
                if (item.Uid == ((byte)timing.S3ToS4).ToString())
                    cmbxPowerUpTimingS3ToS4.SelectedItem = item;
            }
        }

        private void btnWritePowerUpTiming_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            PowerUpTiming timing = new PowerUpTiming();

            timing.StartToS1 = byte.Parse(((ListBoxItem)cmbxPowerUpTimingStToS1.SelectedItem).Uid);
            timing.S1ToS2 = byte.Parse(((ListBoxItem)cmbxPowerUpTimingS1ToS2.SelectedItem).Uid);
            timing.S2ToS3 = byte.Parse(((ListBoxItem)cmbxPowerUpTimingS2ToS3.SelectedItem).Uid);
            timing.S3ToS4 = byte.Parse(((ListBoxItem)cmbxPowerUpTimingS3ToS4.SelectedItem).Uid);

            screen.PowerUpTiming = timing;
        }

        private void btnReadPowerDownTiming_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            PowerDownTiming timing = screen.PowerDownTiming;

            foreach (ListBoxItem item in cmbxPowerDownTimingStToS1.Items)
            {
                if (item.Uid == ((byte)timing.StartToS1).ToString())
                    cmbxPowerDownTimingStToS1.SelectedItem = item;
            }

            foreach (ListBoxItem item in cmbxPowerDownTimingS1ToS2.Items)
            {
                if (item.Uid == ((byte)timing.S1ToS2).ToString())
                    cmbxPowerDownTimingS1ToS2.SelectedItem = item;
            }

            foreach (ListBoxItem item in cmbxPowerDownTimingS2ToS3.Items)
            {
                if (item.Uid == ((byte)timing.S2ToS3).ToString())
                    cmbxPowerDownTimingS2ToS3.SelectedItem = item;
            }

            foreach (ListBoxItem item in cmbxPowerDownTimingS3ToS4.Items)
            {
                if (item.Uid == ((byte)timing.S3ToS4).ToString())
                    cmbxPowerDownTimingS3ToS4.SelectedItem = item;
            }

            foreach (ListBoxItem item in cmbxPowerDownTimingMultiplier.Items)
            {
                if (item.Uid == ((byte)timing.Multiplier).ToString())
                    cmbxPowerDownTimingMultiplier.SelectedItem = item;
            }
        }

        private void btnWritePowerDownTiming_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            PowerDownTiming timing = new PowerDownTiming();

            timing.StartToS1 = byte.Parse(((ListBoxItem)cmbxPowerDownTimingStToS1.SelectedItem).Uid);
            timing.S1ToS2 = byte.Parse(((ListBoxItem)cmbxPowerDownTimingS1ToS2.SelectedItem).Uid);
            timing.S2ToS3 = byte.Parse(((ListBoxItem)cmbxPowerDownTimingS2ToS3.SelectedItem).Uid);
            timing.S3ToS4 = byte.Parse(((ListBoxItem)cmbxPowerDownTimingS3ToS4.SelectedItem).Uid);
            timing.Multiplier = byte.Parse(((ListBoxItem)cmbxPowerDownTimingMultiplier.SelectedItem).Uid);

            screen.PowerDownTiming = timing;
        }

        private void btntestThroughput_Click(object sender, RoutedEventArgs e)
        {
            Screen screen = (Screen)cmbxScreenList.SelectedItem;

            int size = 240000;

            Random rand = new Random();

            byte[] data = (new byte[size]).Select(r => (byte)rand.Next(0,256)).ToArray();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Connector.Action8TestThroughput(screen, data);
            sw.Stop();

            MessageBox.Show("time : " + sw.ElapsedMilliseconds + "ms");
        }
    }
}
