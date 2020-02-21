using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace OjamajoBot
{
    public static class ImageEditor
    {
        public static Font FindFont(
        Graphics g,
        string longString,
        Size Room,
        Font PreferedFont)
        {
            // you should perform some scale functions!!!
            SizeF RealSize = g.MeasureString(longString, PreferedFont);
            float HeightScaleRatio = Room.Height / RealSize.Height;
            float WidthScaleRatio = Room.Width / RealSize.Width;
            float ScaleRatio = (HeightScaleRatio < WidthScaleRatio) ? ScaleRatio = HeightScaleRatio : ScaleRatio = WidthScaleRatio;
            float ScaleFontSize = PreferedFont.Size * ScaleRatio;
            return new Font(PreferedFont.FontFamily, ScaleFontSize, PreferedFont.Style, GraphicsUnit.Pixel);
        }

        public static Font GetAdjustedFont(Graphics GraphicRef, string GraphicString, Font OriginalFont, int ContainerWidth, int MaxFontSize, int MinFontSize, bool SmallestOnFail)
        {
            // We utilize MeasureString which we get via a control instance           
            for (int AdjustedSize = MaxFontSize; AdjustedSize >= MinFontSize; AdjustedSize--)
            {
                Font TestFont = new Font(OriginalFont.Name, AdjustedSize, OriginalFont.Style);

                // Test the string with the new size
                SizeF AdjustedSizeNew = GraphicRef.MeasureString(GraphicString, TestFont);

                if (ContainerWidth > Convert.ToInt32(AdjustedSizeNew.Width))
                {
                    // Good font, return it
                    return TestFont;
                }
            }

            // If you get here there was no fontsize that worked
            // return MinimumSize or Original?
            if (SmallestOnFail)
            {
                return new Font(OriginalFont.Name, MinFontSize, OriginalFont.Style);
            }
            else
            {
                return OriginalFont;
            }
        }

        public static Bitmap convertGreyscale(Image image)
        {
            Bitmap btm = new Bitmap(image);
            for (int i = 0; i < btm.Width; i++)
            {
                for (int j = 0; j < btm.Height; j++)
                {
                    int ser = Convert.ToInt32(btm.GetPixel(i, j).R + btm.GetPixel(i, j).G + btm.GetPixel(i, j).B) / 3;
                    btm.SetPixel(i, j, Color.FromArgb(ser, ser, ser));
                }
            }
            return btm;
        }

        public static Bitmap convertSepia(Image image)
        {
            Bitmap btm = new Bitmap(image);
            for (int i = 0; i < btm.Width; i++)
            {
                for (int j = 0; j < btm.Height; j++)
                {
                    var inputRed = btm.GetPixel(i, j).R;
                    var inputGreen = btm.GetPixel(i, j).G;
                    var inputBlue = btm.GetPixel(i, j).B;

                    var outputRed = (int)(inputRed * .393) + (int)(inputGreen * .769) + (int)(inputBlue * .189);
                    var outputGreen = (int)(inputRed * .349) + (int)(inputGreen * .686) + (int)(inputBlue * .168);
                    var outputBlue = (int)(inputRed * .272) + (int)(inputGreen * .534) + (int)(inputBlue * .131);

                    if (outputRed > 255) outputRed = 255;
                    if (outputGreen > 255) outputGreen = 255;
                    if (outputBlue > 255) outputBlue = 255; 

                    try
                    {
                        btm.SetPixel(i, j, Color.FromArgb(outputRed, outputGreen, outputBlue));
                    }
                    catch(Exception e) { Console.WriteLine(e.ToString()); }
                   
                }
            }
            return btm;
        }

        public static Bitmap MergeBitmaps(Bitmap bmp1, Bitmap bmp2)
        {
            Bitmap result = new Bitmap(Math.Max(bmp1.Width, bmp2.Width),
                                       Math.Max(bmp1.Height, bmp2.Height));
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(bmp2, Point.Empty);
                g.DrawImage(bmp1,new Point(20, Convert.ToInt32(bmp2.Height-(bmp2.Height*0.35))));
            }
            return result;
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            //width = width / 3;
            //height = height / 3;
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static Bitmap convertNegative(Bitmap btm)
        {
            Bitmap bmp = new Bitmap(btm);
            //get image dimension
            int width = bmp.Width;
            int height = bmp.Height;

            //negative
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //get pixel value
                    Color p = bmp.GetPixel(x, y);

                    //extract ARGB value from p
                    int a = p.A;
                    int r = p.R;
                    int g = p.G;
                    int b = p.B;

                    //find negative value
                    r = 255 - r;
                    g = 255 - g;
                    b = 255 - b;

                    //set new ARGB value in pixel
                    bmp.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                }
            }

            return bmp;

        }

    }
}
