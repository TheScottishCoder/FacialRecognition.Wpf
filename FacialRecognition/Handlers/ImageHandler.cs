using Emgu.CV.Structure;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Accord.MachineLearning;
using Emgu.CV.Reg;
using System.Windows.Media;

namespace FacialRecognition.Handlers
{
    public class ImageHandler
    {
        

        public ImageHandler()
        {

        }

        public static Bitmap BitmapImageToBitmap(BitmapImage image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(image));
                enc.Save(ms);
                Bitmap bitmap = new Bitmap(ms);

                return new Bitmap(bitmap);
            }
        }

        public static Image<Bgr, byte> BitmapToEmguImage(Bitmap bitmap)
        {
            Mat mat = new Mat();
            mat = bitmap.ToMat();
            return mat.ToImage<Bgr, byte>();
        }

        public static Image<Bgr, byte> BitmapImageToEmguImage(BitmapImage image)
        {
            var img = BitmapImageToBitmap(image);
            return BitmapToEmguImage(img);
        }

        public static Image<Gray, byte> ResizeImage(Image<Gray, byte> faceImage, int sizeX = 100, int sizeY = 100) =>
            faceImage.Resize(sizeX, sizeY, Emgu.CV.CvEnum.Inter.Cubic);

        public static Image<Gray, byte> NormalizeImage(Image<Gray, byte> faceImage)
        {
            CvInvoke.Normalize(faceImage, faceImage, 0, 255, Emgu.CV.CvEnum.NormType.MinMax);
            return faceImage;
        }

        public static Image<Gray, byte> EqualizeImage(Image<Gray, byte> faceImage)
        {
            CvInvoke.EqualizeHist(faceImage, faceImage);
            return faceImage;
        }

    }
}
