using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Xml;
using FastBitmapLib;

namespace GrayScaleConverterLib
{
    public static class GrayScaleConverter
    {
        public enum ConvertionMethod
        {
            Average,
            AverageBT709,
            AverageBT601,
            Desaturation,
            DecompositionMax,
            DecompositionMin,
            SingleChannelRed,
            SingleChannelGreen,
            SingleChannelBlue,
        }

        public static byte[] FromBitmap(Bitmap bmp, ConvertionMethod method, DitheringMethod dither, bool serpentine, int grayScaleDepth)
        {
            if (!(new[] { 2, 4, 8, 16, 32, 64, 128, 256 }).Contains(grayScaleDepth))
                throw new Exception("GrayScaleDepth must be a power of 2 between 1 and 254");

            byte[] output = new byte[0];

            output = ConvertToGrayscale(bmp, method, grayScaleDepth);

            output = Dither(output, grayScaleDepth, bmp.Width, bmp.Height, dither, serpentine);

            return output;
        }

        public static byte[] ConvertToGrayscale(Bitmap bmp, ConvertionMethod method, int grayScaleDepth)
        {

            byte[] output = new byte[0];

            switch (method)
            {
                case ConvertionMethod.Average:
                    output = ConvAverage(bmp, grayScaleDepth, 1 / 3.0, 1 / 3.0, 1 / 3.0);
                    break;
                case ConvertionMethod.AverageBT709:
                    output = ConvAverage(bmp, grayScaleDepth, 0.2126, 0.7152, 0.0722);
                    break;
                case ConvertionMethod.AverageBT601:
                    output = ConvAverage(bmp, grayScaleDepth, 0.299, 0.587, 0.114);
                    break;
                case ConvertionMethod.Desaturation:
                    output = ConvDesaturation(bmp, grayScaleDepth);
                    break;
                case ConvertionMethod.DecompositionMax:
                    output = ConvDecompositionMax(bmp, grayScaleDepth);
                    break;
                case ConvertionMethod.DecompositionMin:
                    output = ConvDecompositionMin(bmp, grayScaleDepth);
                    break;
                case ConvertionMethod.SingleChannelRed:
                    output = ConvSingleChannel(bmp, grayScaleDepth, Color.Red);
                    break;
                case ConvertionMethod.SingleChannelGreen:
                    output = ConvSingleChannel(bmp, grayScaleDepth, Color.Green);
                    break;
                case ConvertionMethod.SingleChannelBlue:
                    output = ConvSingleChannel(bmp, grayScaleDepth, Color.Blue);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
            }
            return output;
        }

        private static byte[] ConvAverage(Bitmap bmp, int grayScaleDepth, double redRatio = 0.33, double greenRatio = 0.33, double blueRatio = 0.33)
        {
            byte[] output = Enumerable.Repeat(0, bmp.Width * bmp.Height).Select(item => (byte)item).ToArray();

            using (var fastBitmap = bmp.FastLock())
            {
                int counter = 0;
                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {

                        //var pixelColor2 = fastBitmap.GetPixel(x, y);
                        int pixelColor = fastBitmap.GetPixelInt(x, y);

                        byte[] rgbData = BitConverter.GetBytes(pixelColor);

                        byte fullResult = (byte)(rgbData[2] * redRatio + rgbData[1] * greenRatio + rgbData[0] * blueRatio);
                        //byte fullResult = (byte)(pixelColor2.R * redRatio + pixelColor2.G * greenRatio + pixelColor2.B * blueRatio);

                        output[counter] = fullResult;
                        counter++;
                    }
                }
            }

            return output;
        }

        private static byte[] ConvDesaturation(Bitmap bmp, int grayScaleDepth)
        {
            byte[] output = Enumerable.Repeat(0, bmp.Width * bmp.Height).Select(item => (byte)item).ToArray();

            using (var fastBitmap = bmp.FastLock())
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        int outputOffset = (y * bmp.Width + x);

                        Color pixelColor = fastBitmap.GetPixel(x, y);

                        // Gray = ( Max(Red, Green, Blue) + Min(Red, Green, Blue) ) / 2
                        byte fullResult = (byte)((Math.Max(pixelColor.R, Math.Max(pixelColor.G, pixelColor.B)) + Math.Min(pixelColor.R, Math.Min(pixelColor.G, pixelColor.B))) / 2);

                        output[outputOffset] = fullResult;
                    }
                }
            }

            return output;
        }

        private static byte[] ConvDecompositionMax(Bitmap bmp, int grayScaleDepth)
        {
            byte[] output = Enumerable.Repeat(0, bmp.Width * bmp.Height).Select(item => (byte)item).ToArray();

            using (var fastBitmap = bmp.FastLock())
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        int outputOffset = (y * bmp.Width + x);

                        Color pixelColor = fastBitmap.GetPixel(x, y);

                        // Gray = Max(Red, Green, Blue)
                        byte fullResult = (byte)(Math.Max(pixelColor.R, Math.Max(pixelColor.G, pixelColor.B)));

                        output[outputOffset] = fullResult;
                    }
                }
            }

            return output;
        }

        private static byte[] ConvDecompositionMin(Bitmap bmp, int grayScaleDepth)
        {
            byte[] output = Enumerable.Repeat(0, bmp.Width * bmp.Height).Select(item => (byte)item).ToArray();

            using (var fastBitmap = bmp.FastLock())
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        int outputOffset = (y * bmp.Width + x);

                        Color pixelColor = fastBitmap.GetPixel(x, y);

                        // Gray = Min(Red, Green, Blue)
                        byte fullResult = (byte)(Math.Min(pixelColor.R, Math.Min(pixelColor.G, pixelColor.B)));

                        output[outputOffset] = fullResult;
                    }
                }
            }

            return output;
        }

        private static byte[] ConvSingleChannel(Bitmap bmp, int grayScaleDepth, Color color)
        {
            if (color != Color.Red && color != Color.Green && color != Color.Blue)
                throw new Exception("Color must be Red, Green or Blue");

            byte[] output = Enumerable.Repeat(0, bmp.Width * bmp.Height).Select(item => (byte)item).ToArray();

            using (var fastBitmap = bmp.FastLock())
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        int outputOffset = (y * bmp.Width + x);

                        Color pixelColor = fastBitmap.GetPixel(x, y);

                        byte fullResult = 0;

                        if (color == Color.Red)
                            fullResult = pixelColor.R;
                        else if (color == Color.Green)
                            fullResult = pixelColor.G;
                        else if (color == Color.Blue)
                            fullResult = pixelColor.B;

                        output[outputOffset] = fullResult;
                    }
                }
            }

            return output;
        }

        public enum DitheringMethod
        {
            None,
            Simple,
            FloydSteinberg,
            JarvisJudiceNinke,
            Stucki,
            Atkinson,
            Burkes,
            Sierra,
            TwoRowSierra,
            SierraLite

        }

        public static byte[] Dither(byte[] data, int grayScaleDepth, int width, int height, DitheringMethod method, bool serpentine = true, double bleedRatio = 1)
        {
            int[] integerData = data.Select(item => (int)item).ToArray();

            List<byte> realGrayPoints = new List<byte>();
            Dictionary<byte, byte> realGrayToIdx = new Dictionary<byte, byte>();

            double tempGrayPoint = 0;
            byte grayPointIterator = 0;
            while (Math.Round(tempGrayPoint) <= 255)
            {
                realGrayPoints.Add((byte)tempGrayPoint);
                realGrayToIdx.Add((byte)tempGrayPoint, grayPointIterator);

                grayPointIterator++;
                tempGrayPoint += 255.0 / (grayScaleDepth - 1);
            }

            byte[] scaleValueMatrix = new byte[256];
            int[] scaleErrorMatrix = new int[256];
            for (int i = 0; i <= 255; i++)
            {
                var i1 = i;
                var grayErrors = realGrayPoints.Select(item => new { error = i1 - item, gray = item }).OrderBy(item => Math.Abs(item.error));

                scaleValueMatrix[i] = grayErrors.First().gray;
                scaleErrorMatrix[i] = grayErrors.First().error;
            }

            byte ditherDivider = 1;
            byte?[,] ditherMatrix = new byte?[1, 1];
            byte?[,] reverseDitherMatrix = new byte?[1, 1];
            byte xOrigin = 0;
            byte reverseXOrigin = 0;
            byte yOrigin = 0;

            switch (method)
            {
                case DitheringMethod.None:
                    ditherDivider = 1;
                    ditherMatrix = new byte?[1, 1]
                    {
                        {null}
                    };
                    reverseDitherMatrix = new byte?[1, 1]
                    {
                        {null}
                    };
                    xOrigin = 0;
                    reverseXOrigin = 0;
                    yOrigin = 0;
                    break;
                case DitheringMethod.Simple:
                    ditherDivider = 1;
                    ditherMatrix = new byte?[1, 2]
                    {
                        {null, 1}
                    };
                    reverseDitherMatrix = new byte?[1, 2]
                    {
                        {1, null}
                    };
                    xOrigin = 0;
                    reverseXOrigin = 1;
                    yOrigin = 0;
                    break;
                case DitheringMethod.FloydSteinberg:
                    ditherDivider = 16;
                    ditherMatrix = new byte?[2, 3]
                    {
                        {null, null, 7},
                        {   3,    5, 1}
                    };
                    reverseDitherMatrix = new byte?[2, 3]
                    {
                        {7, null, null},
                        {1,    5,    3}
                    };
                    xOrigin = 1;
                    reverseXOrigin = 1;
                    yOrigin = 0;
                    break;
                case DitheringMethod.JarvisJudiceNinke:
                    ditherDivider = 48;
                    ditherMatrix = new byte?[3, 5]
                    {
                        {null, null, null, 7, 5},
                        {   3,    5,    7, 5, 3},
                        {   1,    3,    5, 3, 1}
                    };
                    reverseDitherMatrix = new byte?[3, 5]
                    {
                        {5, 7, null, null, null},
                        {3, 5,    7,    5,    3},
                        {1, 3,    5,    3,    1}
                    };
                    xOrigin = 2;
                    reverseXOrigin = 2;
                    yOrigin = 0;
                    break;
                case DitheringMethod.Stucki:
                    ditherDivider = 42;
                    ditherMatrix = new byte?[3, 5]
                    {
                        {null, null, null, 8, 4},
                        {   2,    4,    8, 4, 2},
                        {   1,    2,    4, 2, 1}
                    };
                    reverseDitherMatrix = new byte?[3, 5]
                    {
                        {4, 8, null, null, null},
                        {2, 4,    8,    4,    2},
                        {1, 2,    4,    2,    1}
                    };
                    xOrigin = 2;
                    reverseXOrigin = 2;
                    yOrigin = 0;
                    break;
                case DitheringMethod.Atkinson:
                    ditherDivider = 8;
                    ditherMatrix = new byte?[3, 4]
                    {
                        {null, null,    1,    1},
                        {   1,    1,    1, null},
                        {null,    1, null, null}
                    };
                    reverseDitherMatrix = new byte?[3, 4]
                    {
                        {   1,    1, null, null},
                        {null,    1,    1,    1},
                        {null, null,    1, null}
                    };
                    xOrigin = 1;
                    reverseXOrigin = 2;
                    yOrigin = 0;
                    break;
                case DitheringMethod.Burkes:
                    ditherDivider = 32;
                    ditherMatrix = new byte?[2, 5]
                    {
                        {null, null, null, 8, 4},
                        {   2,    4,    8, 4, 2}
                    };
                    reverseDitherMatrix = new byte?[2, 5]
                    {
                        {4, 8, null, null, null},
                        {2, 4,    8,    4,    2}
                    };
                    xOrigin = 2;
                    reverseXOrigin = 2;
                    yOrigin = 0;
                    break;
                case DitheringMethod.Sierra:
                    ditherDivider = 32;
                    ditherMatrix = new byte?[3, 5]
                    {
                        {null, null, null, 5,    3},
                        {   2,    4,    5, 1,    2},
                        {null,    2,    3, 2, null}
                    };
                    reverseDitherMatrix = new byte?[3, 5]
                    {
                        {   3, 5, null, null, null},
                        {   2, 1,    5,    4,    2},
                        {null, 2,    3,    2, null}
                    };
                    xOrigin = 2;
                    reverseXOrigin = 2;
                    yOrigin = 0;
                    break;
                case DitheringMethod.TwoRowSierra:
                    ditherDivider = 16;
                    ditherMatrix = new byte?[2, 5]
                    {
                        {null, null, null, 4, 3},
                        {   1,    2,    3, 2, 1}
                    };
                    reverseDitherMatrix = new byte?[2, 5]
                    {
                        {3, 4, null, null, null},
                        {1, 2,    3,    2,    1}
                    };
                    xOrigin = 2;
                    reverseXOrigin = 2;
                    yOrigin = 0;
                    break;
                case DitheringMethod.SierraLite:
                    ditherDivider = 4;
                    ditherMatrix = new byte?[2, 3]
                    {
                        {null, null,    2},
                        {   1,    1, null}
                    };
                    reverseDitherMatrix = new byte?[2, 3]
                    {
                        {   2, null, null},
                        {null,    1,    1}
                    };
                    xOrigin = 1;
                    reverseXOrigin = 1;
                    yOrigin = 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
            }





            for (int y = 0; y < height; y++)
            {

                if (!(serpentine && y % 2 == 0))
                {
                    for (int x = 0; x < width; x++)
                    {
                        int pixelVal = integerData[y * width + x];

                        if (pixelVal > 255)
                            pixelVal = 255;
                        else if (pixelVal < 0)
                            pixelVal = 0;

                        integerData[y * width + x] = realGrayToIdx[scaleValueMatrix[pixelVal]];

                        double error = scaleErrorMatrix[pixelVal];

                        error = Math.Round(error * bleedRatio / ditherDivider);

                        for (int ditherY = 0; ditherY < ditherMatrix.GetLength(0); ditherY++)
                        {
                            int subY = y - yOrigin + ditherY;

                            if (subY >= height)
                                break;
                            else if (subY < 0)
                                continue;

                            for (int ditherX = 0; ditherX < ditherMatrix.GetLength(1); ditherX++)
                            {
                                int subX = x - xOrigin + ditherX;

                                if (subX >= width)
                                    break;
                                else if (subX < 0)
                                    continue;

                                byte? dietherFract = ditherMatrix[ditherY, ditherX];

                                if (dietherFract == null)
                                    continue;

                                double val = integerData[subY * width + subX];

                                val += error * dietherFract ?? 1;

                                integerData[subY * width + subX] = (int)Math.Round(val);
                            }
                        }
                    }
                }
                else
                {
                    for (int x = width - 1; x >= 0; x--)
                    {
                        int pixelVal = integerData[y * width + x];

                        if (pixelVal > 255)
                            pixelVal = 255;
                        else if (pixelVal < 0)
                            pixelVal = 0;

                        integerData[y * width + x] = realGrayToIdx[scaleValueMatrix[pixelVal]];

                        double error = scaleErrorMatrix[pixelVal];

                        error = Math.Round(error * bleedRatio / ditherDivider);

                        for (int ditherY = 0; ditherY < reverseDitherMatrix.GetLength(0); ditherY++)
                        {
                            int subY = y - yOrigin + ditherY;

                            if (subY >= height)
                                break;
                            else if (subY < 0)
                                continue;

                            for (int ditherX = 0; ditherX < reverseDitherMatrix.GetLength(1); ditherX++)
                            {
                                int subX = x - reverseXOrigin + ditherX;

                                if (subX >= width)
                                    break;
                                else if (subX < 0)
                                    continue;

                                byte? dietherFract = reverseDitherMatrix[ditherY, ditherX];

                                if (dietherFract == null)
                                    continue;

                                double val = integerData[subY * width + subX];

                                val += error * dietherFract ?? 1;

                                integerData[subY * width + subX] = (int)Math.Round(val);
                            }
                        }
                    }
                }
            }


            return integerData.Select(item => (byte)item).ToArray();
        }

        public static byte[] DitherSierraLight(byte[] data, int grayScaleDepth, int width, int height)
        {
            int[] integerData = data.Select(item => (int)item).ToArray();

            List<byte> realGrayPoints = new List<byte>();
            Dictionary<byte, byte> realGrayToIdx = new Dictionary<byte, byte>();

            double tempGrayPoint = 0;
            byte grayPointIterator = 0;
            while (Math.Round(tempGrayPoint) <= 255)
            {
                realGrayPoints.Add((byte)tempGrayPoint);
                realGrayToIdx.Add((byte)tempGrayPoint, grayPointIterator);

                grayPointIterator++;
                tempGrayPoint += 255.0 / (grayScaleDepth - 1);
            }

            byte[] scaleValueMatrix = new byte[256];
            int[] scaleErrorMatrix = new int[256];
            for (int i = 0; i <= 255; i++)
            {
                var i1 = i;
                var grayErrors = realGrayPoints.Select(item => new { error = i1 - item, gray = item }).OrderBy(item => Math.Abs(item.error));

                scaleValueMatrix[i] = grayErrors.First().gray;
                scaleErrorMatrix[i] = grayErrors.First().error;
            }

            byte ditherDivider = 4;
            int widthm1 = width - 1;
            int heightm1 = height - 1;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pixelPos = y * width + x;
                    int pixelVal = integerData[pixelPos];

                    if (pixelVal > 255)
                        pixelVal = 255;
                    else if (pixelVal < 0)
                        pixelVal = 0;

                    integerData[pixelPos] = realGrayToIdx[scaleValueMatrix[pixelVal]];

                    int error = scaleErrorMatrix[pixelVal];

                    error = error / ditherDivider;

                    if (x != widthm1)
                    {
                        integerData[pixelPos + 1] += 2 * error;
                    }

                    if (y != heightm1)
                    {
                        integerData[pixelPos + widthm1] += error;
                        if (x != 0)
                        {
                            integerData[pixelPos + width] += error;
                        }
                    }
                }
            }


            return integerData.Select(item => (byte)item).ToArray();
        }


        public static Bitmap GrayToBitmap(byte[] data, int width, int height, int grayScaleDepth)
        {
            List<byte> realGrayPoints = new List<byte>();

            double tempGrayPoint = 0;
            while (Math.Round(tempGrayPoint) <= 255)
            {
                realGrayPoints.Add((byte)tempGrayPoint);

                tempGrayPoint += 255.0 / (grayScaleDepth - 1);
            }

            Bitmap bmp = new Bitmap(width, height);

            using (var fastBitmap = bmp.FastLock())
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        int offset = (y * bmp.Width + x);

                        byte gray = realGrayPoints[data[offset]];

                        fastBitmap.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                    }
                }
            }

            return bmp;
        }

        public static byte[] ReverseGrayScale(byte[] data, int grayScaleDepth)
        {
            Dictionary<byte, byte> reverseScale = new Dictionary<byte, byte>();

            byte reversScaleCount = (byte)(grayScaleDepth - 1);
            for (byte i = 0; i < grayScaleDepth; i++)
            {
                reverseScale.Add(i, reversScaleCount);
                reversScaleCount--;
            }

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = reverseScale[data[i]];
            }

            return data;
        }

        public static byte[] CompactArray(byte[] data, int grayScaleDepth)
        {
            Dictionary<int, int> pixPerByte = new Dictionary<int, int>()
            {
                {2, 8 },
                {4, 4 },
                {8, 2 },
                {16, 2 },
                {32, 1 },
                {64, 1 },
                {128, 1 },
                {256, 1 }
            };

            Dictionary<int, int> bitPerByte = new Dictionary<int, int>()
            {
                {2, 1 },
                {4, 2 },
                {8, 3 },
                {16, 4 },
                {32, 5 },
                {64, 6 },
                {128, 7 },
                {256, 8 }
            };


            BitArray outputData = new BitArray(data.Length * bitPerByte[grayScaleDepth]);



            int offset = 0;

            int rollingCount = 0;

            foreach (byte b in data)
            {
                outputData[offset] = new BitArray(new byte[] { b })[0];


                offset++;
                rollingCount++;

                if (rollingCount > pixPerByte[grayScaleDepth])
                    rollingCount = 0;
            }


            byte[] ret = new byte[(outputData.Length - 1) / 8 + 1];
            outputData.CopyTo(ret, 0);


            return ret;

        }
    }
}
