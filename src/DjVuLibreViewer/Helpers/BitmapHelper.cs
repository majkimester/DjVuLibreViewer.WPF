using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DjVuLibreViewer.Helpers
{
    internal static class BitmapHelper
    {
        public static Bitmap CreateBitmapFromRgb24(byte[] rgbBytes, int width, int height, float dpiX, float dpiY)
        {
            // Create and fill Bitmap
            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            bitmap.SetResolution(dpiX, dpiY);

            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            int stride = data.Stride;
            IntPtr ptr = data.Scan0;

            for (int y = 0; y < height; y++)
            {
                Marshal.Copy(rgbBytes, y * width * 3, ptr + y * stride, width * 3);
            }

            bitmap.UnlockBits(data);
            return bitmap;
        }

        public static BitmapSource CreateFromRgb24(byte[] rgbBytes, int width, int height, float dpiX, float dpiY)
        {
            int bytesPerPixel = 3;
            int stride = width * bytesPerPixel;

            return BitmapSource.Create(
                width,
                height,
                dpiX: dpiX,
                dpiY: dpiY,
                PixelFormats.Rgb24,
                palette: null,
                pixels: rgbBytes,
                stride: stride
            );
        }
    }
}
