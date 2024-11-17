using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Avalonia.Media.Imaging;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace MockupImageProccessing.Extension;

public static class ImageExtensions
{
    public static Bitmap ConvertToAvaloniaBitmap(this System.Drawing.Bitmap bitmap)
    {
        if (bitmap == null)
            return null;
        System.Drawing.Bitmap bitmapTmp = bitmap;
        var bitmapdata = bitmapTmp.LockBits(new Rectangle(0, 0, bitmapTmp.Width, bitmapTmp.Height),
            ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        Bitmap bitmap1 = new Bitmap(Avalonia.Platform.PixelFormat.Bgra8888, Avalonia.Platform.AlphaFormat.Premul,
            bitmapdata.Scan0,
            new Avalonia.PixelSize(bitmapdata.Width, bitmapdata.Height),
            new Avalonia.Vector(96, 96),
            bitmapdata.Stride);
        bitmapTmp.UnlockBits(bitmapdata);
        bitmapTmp.Dispose();
        return bitmap1;
    }

    /// <summary>
    /// copy new received bitmap data to reusable bitmap that already bind to avalonia control
    /// condition: reusable bitmap must have the same size 
    /// </summary>
    /// <param name="bitmap"></param>
    /// <param name="reusableBitmap"></param>
    public static void ConvertToAvaloniaBitmap(this System.Drawing.Bitmap bitmap, WriteableBitmap reusableBitmap)
    {
        if (reusableBitmap == null)
            return;
        var data = BitmapToByteArray(bitmap);
        using (var lockedBitmap = reusableBitmap.Lock())
        {
            Marshal.Copy(data, 0, lockedBitmap.Address, data.Length);
        }
    }
    private static byte[] BitmapToByteArray(System.Drawing.Bitmap bitmap)
    {
        var bitmapdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        int bytes = Math.Abs(bitmapdata.Stride) * bitmap.Height;
        byte[] byteArray = new byte[bytes];
        Marshal.Copy(bitmapdata.Scan0, byteArray, 0, bytes);
        bitmap.UnlockBits(bitmapdata);
        return byteArray;
    }
}

