using System.Drawing;
using ScreenConnection;

namespace MasterModuleCommon
{
    public static class GraphicHelper
    {
        public static Point[] ComputeTargetPoints(Size target, Size source, Rotation targetRotation)
        {

            Point[] pointArr = new Point[3];

            pointArr[0] = new Point();
            pointArr[1] = new Point();
            pointArr[2] = new Point();


            switch (targetRotation)
            {

                case Rotation.DEG_0:
                case Rotation.DEG_180:
                    {
                        int newWidth = 0;
                        int newHeight = 0;

                        bool isSourceHorizontalStretch = (float)target.Width / (float)target.Height < (float)source.Width / (float)source.Height;

                        if (isSourceHorizontalStretch)
                        {
                            newWidth = target.Width;
                            newHeight = (int)((float)target.Width / ((float)source.Width / (float)source.Height));
                        }
                        else
                        {
                            newWidth = (int)((float)target.Height * ((float)source.Width / (float)source.Height));
                            newHeight = target.Height;
                        }

                        Point tl = new Point();
                        Point br = new Point();

                        tl.X = (target.Width - newWidth) / 2;
                        tl.Y = (target.Height - newHeight) / 2;

                        br.X = (target.Width - newWidth) / 2 + newWidth;
                        br.Y = (target.Height - newHeight) / 2 + newHeight;

                        if (targetRotation == Rotation.DEG_180)
                        {
                            Point tempCorner = tl;
                            tl = br;
                            br = tempCorner;
                        }

                        pointArr[0].X = tl.X;
                        pointArr[0].Y = tl.Y;

                        pointArr[1].X = br.X;
                        pointArr[1].Y = tl.Y;

                        pointArr[2].X = tl.X;
                        pointArr[2].Y = br.Y;

                        break;
                    }
                case Rotation.DEG_90:
                case Rotation.DEG_270:
                    {
                        int newWidth = 0;
                        int newHeight = 0;

                        bool isSourceHorizontalStretch = (float)target.Height / (float)target.Width < (float)source.Width / (float)source.Height;

                        if (isSourceHorizontalStretch)
                        {
                            newWidth = target.Height;
                            newHeight = (int)((float)target.Height / ((float)source.Width / (float)source.Height));
                        }
                        else
                        {
                            newWidth = (int)((float)target.Width * ((float)source.Width / (float)source.Height));
                            newHeight = target.Width;
                        }


                        Point tl = new Point();
                        Point br = new Point();


                        tl.X = (target.Width - newHeight) / 2;
                        tl.Y = (target.Height - newWidth) / 2 + newWidth;

                        br.X = (target.Width - newHeight) / 2 + newHeight;
                        br.Y = (target.Height - newWidth) / 2;

                        if (targetRotation == Rotation.DEG_270)
                        {
                            Point tempCorner = tl;
                            tl = br;
                            br = tempCorner;
                        }

                        pointArr[0].X = tl.X;
                        pointArr[0].Y = tl.Y;

                        pointArr[1].X = tl.X;
                        pointArr[1].Y = br.Y;

                        pointArr[2].X = br.X;
                        pointArr[2].Y = tl.Y;

                        break;
                    }
            }

            return pointArr;
        }
    }
}
