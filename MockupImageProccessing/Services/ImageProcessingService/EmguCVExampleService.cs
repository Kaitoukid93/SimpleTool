using System;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Point = Avalonia.Point;

namespace MockupImageProccessing.Services.ImageProcessingService;

public class EmguCVExampleService
{
    public EmguCVExampleService()
    {
    }

    public Bitmap ProcessImage(string imagePath)
    {
        try
        {
            using (var image = new Image<Bgr, byte>(imagePath))
            {
                int width = image.Width;
                int height = image.Height;
                //convert image to Mat
                Mat mat = image.Mat;
                //Draw red rectangle at center with quarter image size
                int rectWidth = width / 4;
                int rectHeight = height / 4;
                Point rectTopLeft= new Point((width - rectWidth) / 2, (height - rectHeight) / 2);
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle((int)rectTopLeft.X,(int)rectTopLeft.Y, rectWidth,rectHeight);
                CvInvoke.Rectangle(mat, rect, new MCvScalar(0, 0, 255), 2);
                //convert back to bitmap 
                return mat.ToBitmap();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
}