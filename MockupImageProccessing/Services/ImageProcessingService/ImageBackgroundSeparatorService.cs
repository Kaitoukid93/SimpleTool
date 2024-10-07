using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Avalonia.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Bitmap = System.Drawing.Bitmap;

namespace MockupImageProccessing.Services.ImageProcessingService;

public class ImageBackgroundSeparatorService
{
    //the ratio represent most right point position relative to the bottom
    private double right2BotRatio = 395d / 540d;

    //the ratio represent top center  point position relative to the most left point
    private double left2TopRatio = 270d / 314d;

    //the ratio represent width height ratio of the object
    private double widthHeightRatio = 512d / 672d;
    private Avalonia.Size _imageSize = new Avalonia.Size(1000, 10000);
    private Point[] _outline;
    private Point _bottomPoint;
    private Point _topPoint;
    private Point _mostLeftPoint;
    private Point _mostRightPoint;
    private Rectangle _imageBoundingBox;
    private Rectangle _desiredObject;
    private Point _centerTop;
    private VectorOfVectorOfPoint _contours;

    public ImageBackgroundSeparatorService()
    {
    }

    private List<string> _imagesPath;

    public void Init(List<string> imagesPath)
    {
        _imagesPath = imagesPath;
    }

    private VectorOfVectorOfPoint FindContour(Image<Bgr, byte> image)
    {
        var grayImage = image.Convert<Gray, byte>();
        var leftRegion = grayImage.GetSubRect(new Rectangle(0, 0, 100, image.Height / 2));
        var average = leftRegion.GetAverage();
        //Calculate dynamic threshold
        var minimumThresold = 200;
        var maximumThresold = 250;
        var intensityRatio = average.Intensity / 255;
        var dynamicThresold = intensityRatio * 50;
        // Threshold to separate object from background,
        CvInvoke.Threshold(grayImage, grayImage, minimumThresold + dynamicThresold, 255, ThresholdType.BinaryInv);

        // Find contours
        var contours = new VectorOfVectorOfPoint();
        CvInvoke.FindContours(grayImage, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
        return contours;
    }

    private Point[] GetContourOutline(VectorOfVectorOfPoint contours)
    {
        // Find the largest contour (assuming it's the 2 shirts back to back)
        double maxArea = 0;
        int maxContourIndex = -1;
        List<int> largestContours = new List<int>();
        //find and order contour 
        for (int i = 0; i < contours.Size; i++)
        {
            double area = CvInvoke.ContourArea(contours[i]);
            // var minXPoint = MinXPoint(contours[i]);
            if (area > maxArea)
            {
                maxArea = area;
                maxContourIndex = i;
                largestContours.Insert(0, i);
            }
        }

        if (maxContourIndex >= 0)
        {
            var outline = contours[maxContourIndex].ToArray();
            return outline;
        }
        return null;
    }


    private Point FindMostLeftPoint()
    {
        Rectangle boundingBox = CvInvoke.BoundingRectangle(_outline);
        Point mostLeftPoint = Point.Empty;
        int mostLeftX = int.MaxValue;
        var listLeftPoints = new List<Point>();
        foreach (Point point in _outline)
        {
            if (point.X < mostLeftX)
            {
                mostLeftX = point.X;
                mostLeftPoint = point;
            }
        }

        foreach (Point point in _outline)
        {
            if (point.X == mostLeftX)
            {
                listLeftPoints.Add(point);
            }
        }

        if (listLeftPoints.Count > 1)
        {
            mostLeftPoint = listLeftPoints[listLeftPoints.Count / 2];
        }

        return mostLeftPoint;
    }

    private Point FindBottomPoint()
    {
        Point bottomPoint = Point.Empty;
        int lowestY = int.MinValue;
        var listHighestPoints = new List<Point>();
        foreach (Point point in _outline)
        {
            if (point.Y > lowestY)
            {
                lowestY = point.Y;

                bottomPoint = point;
            }
        }

        foreach (Point point in _outline)
        {
            if (point.Y == lowestY)
            {
                listHighestPoints.Add(point);
            }
        }

        if (listHighestPoints.Count > 1)
        {
            bottomPoint = listHighestPoints[listHighestPoints.Count / 2];
        }

        return bottomPoint;
    }

    /// <summary>
    /// get desired object's bounding box from provided point and ration
    /// </summary>
    private Rectangle GetObjectBoundingBox(Point mostLeftPoint, Point bottomPoint, Rectangle boundingBox)
    {
        var right = mostLeftPoint.X + (bottomPoint.Y - mostLeftPoint.Y) / right2BotRatio;
        var centerX = (right - mostLeftPoint.X) / 2;
        var top = mostLeftPoint.Y - (centerX) / left2TopRatio;
        if (top < boundingBox.Top)
            top = boundingBox.Top;
        _centerTop = new Point((int)centerX, (int)top);

        return new Rectangle(mostLeftPoint.X, (int)top, (int)(right - mostLeftPoint.X),
            (int)(bottomPoint.Y - top));
    }

    /// <summary>
    /// adjust image to pixel perfect
    /// </summary>
    private int MicroAdjustment(Image<Bgr, byte> image)
    {
        //get micro different from top
        //get top point contour that below or above nearest to center top
        //crop half left image
        image.ROI = new Rectangle(0, 0, _desiredObject.Width / 2 + _desiredObject.X, image.Height);
        //find current image contour
        var contours = FindContour(image);
        var outline = GetContourOutline(contours);
        var boundingBox = CvInvoke.BoundingRectangle(outline);
        int heightDiff = 0;
        if (boundingBox.Top != _centerTop.Y)
        {
            heightDiff = boundingBox.Top - _centerTop.Y;
            Console.WriteLine("Feedback height : " + heightDiff);
            _desiredObject.Y += heightDiff;
            _desiredObject.Height -= heightDiff;
        }

        else
        {
            Console.WriteLine("center top match");
            return 0;
        }

        var newWidth = _desiredObject.Height * widthHeightRatio;
        Console.WriteLine("Feedback width: " + (newWidth - _desiredObject.Width));
        _desiredObject.Width = (int)newWidth;
        return boundingBox.Top - _centerTop.Y;
    }

    public Bitmap ProcessImage(string imagePath, bool showContour, bool showRectangle, double rb,
        double lt, double wh, Avalonia.Size imageSize)
    {
        right2BotRatio = rb;
        left2TopRatio = lt;
        widthHeightRatio = wh;
        _imageSize = imageSize;
        using (var image = new Image<Bgr, byte>(imagePath))
        {
            int width = image.Width;
            int height = image.Height;
            //find contour
            _contours = FindContour(image);
            //find outline;
            _outline = GetContourOutline(_contours);
            //get contour bounding box
            _imageBoundingBox = CvInvoke.BoundingRectangle(_outline);
            //get most left point
            _mostLeftPoint = FindMostLeftPoint();
            //get bottom point
            _bottomPoint = FindBottomPoint();
            //get object desired bounding box;
            _desiredObject = GetObjectBoundingBox(_mostLeftPoint, _bottomPoint, _imageBoundingBox);
            //execute micro adjust to fine tune the image, this act like a feedback signal
            int feedback = MicroAdjustment(image);
            _centerTop = new Point(_centerTop.X, _desiredObject.Y);
            //drawing for debugging
            image.ROI = new Rectangle(0, 0, width, height);
            if (showContour)
                CvInvoke.DrawContours(image, _contours, _contours.Size - 1, new MCvScalar(255, 0, 0));
            if (showRectangle)
            {
                CvInvoke.Rectangle(image, new Rectangle(_centerTop.X - 5, _centerTop.Y - 5, 10, 10),
                    new MCvScalar(0, 0, 255), 1);
                CvInvoke.Rectangle(image, new Rectangle(_mostLeftPoint.X - 5, _mostLeftPoint.Y - 5, 10, 10),
                    new MCvScalar(0, 0, 255), 1);
                CvInvoke.Rectangle(image, new Rectangle(_mostRightPoint.X - 5, _mostRightPoint.Y - 5, 10, 10),
                    new MCvScalar(0, 0, 255), 1);
                CvInvoke.Rectangle(image, new Rectangle(_bottomPoint.X - 5, _bottomPoint.Y - 5, 10, 10),
                    new MCvScalar(0, 0, 255), 1);
                CvInvoke.Rectangle(image, _desiredObject, new MCvScalar(0, 0, 255), 1);
            }

            //resize image keeping aspect ratio
            double ratio = (double)_desiredObject.Width / (double)_desiredObject.Height;
            //calculate new width
            var newWidth = (double)_imageSize.Height * ratio;
            //create new image with new size
            var resizedImage = new Image<Bgr, byte>(new Size((int)newWidth, (int)_imageSize.Height));
            //crop original image 
            image.ROI = _desiredObject;
            //resize cropped image
            CvInvoke.Resize(image, resizedImage, new Size((int)newWidth, (int)_imageSize.Height), 0, 0, Inter.Linear);
            //Create a blank image with final size
            Image<Bgr, byte> finalImage = new Image<Bgr, byte>((int)_imageSize.Width, (int)_imageSize.Height);
            //Set its ROI to the same as Mask and in the centre of the image (you may wish to change this)
            finalImage.ROI = new Rectangle((finalImage.Width - resizedImage.Width) / 2,
                (finalImage.Height - resizedImage.Height) / 2, resizedImage.Width, resizedImage.Height);
            //Copy the mask to the return image
            CvInvoke.cvCopy(resizedImage, finalImage, IntPtr.Zero);
            //Reset the return image ROI so it has the same dimensions
            finalImage.ROI = new Rectangle(0, 0, (int)_imageSize.Width, (int)_imageSize.Height);
            //Return the image instead of the mask
            return finalImage.ToBitmap();
        }
    }
}