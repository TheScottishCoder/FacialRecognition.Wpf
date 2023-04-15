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

namespace FacialRecognition.Helper
{
    public class ImageHandler
    {
        // Haar Cascade
        private static CascadeClassifier _classifer;

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

        public static Emgu.CV.Image<Bgr, byte> BitmapToEmguImage(Bitmap bitmap)
        {
            Mat mat = new Mat();
            mat = BitmapExtension.ToMat(bitmap);
            return mat.ToImage<Bgr, byte>();
        }

        public static Emgu.CV.Image<Bgr, byte> BitmapImageToEmguImage(BitmapImage image)
        {
            var img = BitmapImageToBitmap(image);
            return BitmapToEmguImage(img);
        }

        public static Image<Bgr, Byte> HaarCascadeFaceDetect(BitmapImage image)
        {
            CheckClassifer();

            Emgu.CV.Image<Bgr, Byte> img = ImageHandler.BitmapImageToEmguImage(image);
            var imgGray = img.Convert<Gray, byte>();

            // scaleFactor = Lower means more accurate results but longer processing times
            Rectangle[] faces = _classifer.DetectMultiScale(imgGray, 1.1, 4);

            foreach (var face in faces)
            {
                img.Draw(face, new Bgr(0, 0, 255), 2);
            }

            return img;
        }

        public static Image<Gray, Byte> HaarCascadeFaceExtract(BitmapImage image)
        {
            CheckClassifer();

            Emgu.CV.Image<Bgr, Byte> img = ImageHandler.BitmapImageToEmguImage(image);
            var imgGray = img.Convert<Gray, byte>();

            // scaleFactor = Lower means more accurate results but longer processing times
            Rectangle[] faces = _classifer.DetectMultiScale(imgGray, 1.1, 4);

            Rectangle largestFace = faces.OrderByDescending(f => f.Size.Width * f.Size.Height).FirstOrDefault();

            var faceImage = imgGray.Copy(largestFace);
            faceImage = faceImage.Resize(100, 100, Emgu.CV.CvEnum.Inter.Cubic);
            CvInvoke.Normalize(faceImage, faceImage, 0, 255, Emgu.CV.CvEnum.NormType.MinMax);

            return faceImage;
        }

        private static void CheckClassifer()
        {
            string faceCascadePath = Path.GetFullPath(@"data/haarcascades_frontalface_default.xml");

            if (_classifer == null)
                _classifer = new CascadeClassifier(faceCascadePath);
        }
    }
}
