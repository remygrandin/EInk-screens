using System.Collections.Generic;
using MasterModuleCommon;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using NLog;
using ScreenConnection;

namespace FileGraphics
{
    public class FileGraphicsProvider : GraphicProvider
    {
        private string _basePath;

        private string[] _filesPaths = new string[0];

        private readonly string[] _listedExtentions = new[]
        {
            "bmp",
            "png",
            "jpeg",
            "jpg",
            "gif"
        };

        private int _pos = 0;

        private Logger _logger;

        public override void Init(Logger logger, IList<MasterModuleCommon.KeyValuePair<string, string>> parameters)
        {
            _logger = logger;

            _pos = 0;
            _basePath = @"C:\MasterControl\TestImages\";

            _filesPaths = Directory.EnumerateFiles(_basePath, "*", SearchOption.AllDirectories)
                .Where(item => _listedExtentions.Contains(item.Split('.').Last().ToLower())).ToArray();
        }

        


        public override Bitmap GetNextGraphic(Screen target)
        {
            Image source = Image.FromFile(_filesPaths[_pos]);

            Bitmap newImage = new Bitmap(Screen.Width, Screen.Height);

            using (Graphics gr = Graphics.FromImage(newImage))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBilinear;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;

                gr.Clear(Color.White);

                var points = GraphicHelper.ComputeTargetPoints(new Size(Screen.Width, Screen.Height),
                    new Size(source.Width, source.Height), target.Rotation);

                //gr.DrawPolygon(new Pen(Color.Crimson),points );

                gr.DrawImage(source, points);

                gr.Save();
            }

            _pos++;

            if (_pos >= _filesPaths.Length)
                _pos = 0;

            return newImage;
        }
    }
}
