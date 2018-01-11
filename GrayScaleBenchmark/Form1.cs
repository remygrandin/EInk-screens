using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GrayScaleConverterLib;

namespace GrayScaleBenchmark
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void render()
        {
            if (LSTB_Files.SelectedItem == null || String.IsNullOrWhiteSpace(LSTB_Files.SelectedItem.ToString()))
                return;

            Bitmap bmp = new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LSTB_Files.SelectedItem.ToString()));

            int depth = int.Parse(CMBX_GrayScale.SelectedItem.ToString());

            GrayScaleConverter.ConvertionMethod method = (GrayScaleConverter.ConvertionMethod)Enum.Parse(typeof(GrayScaleConverter.ConvertionMethod), CMBX_Method.SelectedItem.ToString());
            GrayScaleConverter.DitheringMethod dithering = (GrayScaleConverter.DitheringMethod)Enum.Parse(typeof(GrayScaleConverter.DitheringMethod), CMBX_Dithering.SelectedItem.ToString());
            bool serpentine = CHKB_Serpentine.Checked;


            byte[] grayData = GrayScaleConverter.FromBitmap(bmp, method, dithering, serpentine, depth);

            Bitmap grayBmp = GrayScaleConverter.GrayToBitmap(grayData, bmp.Width, bmp.Height, depth);

            PCBX_Output.Image = grayBmp;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            render();










            /*
            int count = 1;

            if (!Directory.Exists(@".\out\"))
                Directory.CreateDirectory(@".\out\");






            foreach (object convValue in Enum.GetValues(typeof(GrayScaleConverter.ConvertionMethod)))
            {
                foreach (object ditherValue in Enum.GetValues(typeof(GrayScaleConverter.DitheringMethod)))
                {
                    byte[] grayData = GrayScaleConverter.FromBitmap(bmp, (GrayScaleConverter.ConvertionMethod)convValue, (GrayScaleConverter.DitheringMethod)ditherValue, depth);

                    Bitmap grayBmp = GrayScaleConverter.GrayToBitmap(grayData, bmp.Width, bmp.Height, depth);

                    string convName = Enum.GetName(typeof(GrayScaleConverter.ConvertionMethod), convValue);
                    string ditherName = Enum.GetName(typeof(GrayScaleConverter.DitheringMethod), ditherValue);

                    string strCount = count.ToString().PadLeft(2, '0');

                    //grayBmp.Save(@".\out\out-" + strCount + "-" + convName + "-" + ditherName + ".png", ImageFormat.Png);

                    PCBX_Output.Image = grayBmp;

                    count++;
                }
            }*/
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DirectoryInfo di = new DirectoryInfo(".");

            CMBX_Method.DataSource = Enum.GetValues(typeof(GrayScaleConverter.ConvertionMethod));
            CMBX_Dithering.DataSource = Enum.GetValues(typeof(GrayScaleConverter.DitheringMethod));

            CMBX_Method.SelectedIndex = 0;
            CMBX_Dithering.SelectedIndex = 0;

            foreach (FileInfo fileInfo in di.GetFiles("*.png"))
            {
                LSTB_Files.Items.Add(fileInfo.Name);
            }

            LSTB_Files.SelectedIndex = 0;
        }

        private void LSTB_Files_SelectedIndexChanged(object sender, EventArgs e)
        {
            render();

        }

        private void CMBX_GrayScale_SelectedIndexChanged(object sender, EventArgs e)
        {
            render();
        }

        private void CMBX_Method_SelectedIndexChanged(object sender, EventArgs e)
        {
            render();
        }

        private void CMBX_Dithering_SelectedIndexChanged(object sender, EventArgs e)
        {
            render();
        }

        private void CHKB_Serpentine_CheckedChanged(object sender, EventArgs e)
        {
            render();
        }

        private void BTN_GenAll_Click(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LSTB_Files.SelectedItem.ToString()));

            int count = 1;

            if (!Directory.Exists(@".\out\"))
                Directory.CreateDirectory(@".\out\");

            foreach (int grayScale in new List<int>() { 2, 4, 8, 16, 32, 64, 128, 256 })
            {
                foreach (GrayScaleConverter.ConvertionMethod convValue in Enum.GetValues(typeof(GrayScaleConverter.ConvertionMethod)))
                {
                    foreach (GrayScaleConverter.DitheringMethod ditherValue in Enum.GetValues(typeof(GrayScaleConverter.DitheringMethod)))
                    {
                        foreach (bool serpentine in new List<bool>() { false, true })
                        {
                            byte[] grayData = GrayScaleConverter.FromBitmap(bmp, convValue, ditherValue, serpentine, grayScale);

                            Bitmap grayBmp = GrayScaleConverter.GrayToBitmap(grayData, bmp.Width, bmp.Height, grayScale);

                            string convName = Enum.GetName(typeof(GrayScaleConverter.ConvertionMethod), convValue);
                            string ditherName = Enum.GetName(typeof(GrayScaleConverter.DitheringMethod), ditherValue);

                            string strCount = count.ToString().PadLeft(3, '0');

                            grayBmp.Save(@".\out\" + LSTB_Files.SelectedItem.ToString() + "-" + strCount 
                                + "-" + grayScale 
                                + "-"+ convName 
                                + "-" + ditherName 
                                + "-" + (serpentine ? "Serp" : "Regular") 
                                + ".png", ImageFormat.Png);

                            count++;
                        }
                    }
                }
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Bitmap bmp = new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LSTB_Files.SelectedItem.ToString()));

            Bitmap newImage = new Bitmap(800, 600);

            using (Graphics gr = Graphics.FromImage(newImage))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.DrawImage(bmp, new Rectangle(0, 0, 800, 600));

                gr.Save();
            }

            GrayScaleConverter.ConvertionMethod method = (GrayScaleConverter.ConvertionMethod)Enum.Parse(typeof(GrayScaleConverter.ConvertionMethod), CMBX_Method.SelectedItem.ToString());
            GrayScaleConverter.DitheringMethod dithering = (GrayScaleConverter.DitheringMethod)Enum.Parse(typeof(GrayScaleConverter.DitheringMethod), CMBX_Dithering.SelectedItem.ToString());
            sw.Stop();

            long phase1ms = sw.ElapsedMilliseconds;

            List<Tuple<long, long, long>> timing = new List<Tuple<long, long, long>>();

            for (int i = 0; i < 10; i++)
            {
                sw.Restart();
                
                byte[] grayData = GrayScaleConverter.ConvertToGrayscale(newImage, method, 8);
                sw.Stop();

                long grayms = sw.ElapsedMilliseconds;

                sw.Restart();

                grayData = GrayScaleConverter.DitherSierraLight(grayData, 8, newImage.Width, newImage.Height);

                sw.Stop();
                long diterms = sw.ElapsedMilliseconds;

                sw.Restart();

                byte[] output = new byte[(int)Math.Ceiling(grayData.Length / 2.0)];

                int counter = 0;

                for (int j = 0; j < grayData.Length; j += 2)
                {
                    output[counter] = (byte)(grayData[j] << 4 | grayData[j + 1]);

                    counter++;
                }

                grayData = output;

                sw.Stop();

                long compressms = sw.ElapsedMilliseconds;



                timing.Add(new Tuple<long, long, long>(grayms, compressms, diterms));
            }

            MessageBox.Show("Phase 1 : " + phase1ms + "ms " + Environment.NewLine +
                "Phase gray : min : " + timing.Select(item => item.Item1).Min() + "ms | max : " + timing.Select(item => item.Item1).Max() + "ms | avg : " + timing.Select(item => item.Item1).Average() + "ms" + Environment.NewLine +
                            "Phase compress : min : " + timing.Select(item => item.Item2).Min() + "ms | max : " + timing.Select(item => item.Item2).Max() + "ms | avg : " + timing.Select(item => item.Item2).Average() + "ms" + Environment.NewLine +
                            "Phase diter : min : " + timing.Select(item => item.Item3).Min() + "ms | max : " + timing.Select(item => item.Item3).Max() + "ms | avg : " + timing.Select(item => item.Item3).Average() + "ms"


                );

        }
    }
}
