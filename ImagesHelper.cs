using System.Drawing;
using static System.Net.Mime.MediaTypeNames;
using System.IO;
using Image = System.Drawing.Image;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace Accountable
{
    public class ImagesHelper
    {
        static public void SaveFormFileImageTo(IFormFile image, string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                image.CopyTo(fs);
            }
        }

        [SupportedOSPlatform("windows")]
        static public void ResizeImageFromTo(string pathFrom, string pathTo, int width, int height)
        {
            Image original = Image.FromFile(pathFrom);
            var destRect = new Rectangle(0, 0, width, height);
            var destImg = new Bitmap(width, height);
            destImg.SetResolution(original.HorizontalResolution, original.VerticalResolution);
            using (var graphics = Graphics.FromImage(destImg))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(original, destRect, 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            destImg.Save(pathTo);
        }
    }
}
