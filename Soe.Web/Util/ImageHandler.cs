using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace SoftOne.Soe.Web.Util
{
    public class ImageHandler
    {
        public static byte[] Resize(byte[] imageBytes, int height, int width)
        {
            byte[] imageOut = null;

            try
            {
                Bitmap loBMP;
                using (var streamImage = new MemoryStream(imageBytes))
                {
                    loBMP = new Bitmap(streamImage);
                }

                ImageFormat loFormat = loBMP.RawFormat;

                decimal lnRatio;
                int newWidth = 0;
                int newHeight = 0;

                if (loBMP.Height > height && loBMP.Width > width)
                {
                    decimal widthToHeightRatio = (decimal)width / (decimal)height;
                    if ((loBMP.Width / widthToHeightRatio) > loBMP.Height)
                        newWidth = width;
                    else
                        newHeight = height;
                }
                else if (loBMP.Height <= height && loBMP.Width > width)
                    newWidth = width;
                else
                    newHeight = height;

                if (newWidth > 0)
                {
                    lnRatio = (decimal)newWidth / loBMP.Width;
                    decimal lnTemp = loBMP.Height * lnRatio;
                    if ((int)lnTemp > height)
                    {
                        newHeight = height;
                        lnRatio = (decimal)newHeight / loBMP.Height;
                        lnTemp = loBMP.Width * lnRatio;
                        newWidth = (int)lnTemp;
                    }
                    else
                        newHeight = (int)lnTemp;

                }
                else
                {
                    lnRatio = (decimal)newHeight / loBMP.Height;
                    decimal lnTemp = loBMP.Width * lnRatio;
                    if ((int)lnTemp > width)
                    {
                        newWidth = width;
                        lnRatio = (decimal)newWidth / loBMP.Width;
                        lnTemp = loBMP.Height * lnRatio;
                        newHeight = (int)lnTemp;
                    }
                    else
                        newWidth = (int)lnTemp;
                }

                var bmpOut = new Bitmap(newWidth, newHeight);
                Graphics g = Graphics.FromImage(bmpOut);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.FillRectangle(Brushes.White, 0, 0, newWidth, newHeight);
                g.DrawImage(loBMP, 0, 0, newWidth, newHeight);
                loBMP.Dispose();

                using (var stream = new MemoryStream())
                {
                    bmpOut.Save(stream, ImageFormat.Jpeg);
                    imageOut = stream.ToArray();
                    bmpOut.Dispose();
                }
            }
            catch
            {
                return null;
            }

            return imageOut;
        }
    }
}