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

        /// <summary>
        /// Convert BitmapImage to Bitmap
        /// </summary>
        /// <param name="image"></param>
        /// <returns>Bitmap object</returns>
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

        /// <summary>
        /// Convert Bitmap to Emgu.CV Image
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns>Emgu.CV Image object</returns>
        public static Image<Bgr, byte> BitmapToEmguImage(Bitmap bitmap)
        {
            Mat mat = new Mat();
            mat = bitmap.ToMat();
            return mat.ToImage<Bgr, byte>();
        }

        /// <summary>
        /// Convert Bitmap Image to Emgu Image
        /// </summary>
        /// <param name="image"></param>
        /// <returns>Emgu.CV Image object</returns>
        public static Image<Bgr, byte> BitmapImageToEmguImage(BitmapImage image)
        {
            var img = BitmapImageToBitmap(image);
            return BitmapToEmguImage(img);
        }

        /// <summary>
        /// Resize image by default to 100x100
        /// </summary>
        /// <param name="faceImage"></param>
        /// <param name="sizeX"></param>
        /// <param name="sizeY"></param>
        /// <returns>Emgu.CV Image Object</returns>
        public static Image<Gray, byte> ResizeImage(Image<Gray, byte> faceImage, int sizeX = 100, int sizeY = 100) =>
            faceImage.Resize(sizeX, sizeY, Emgu.CV.CvEnum.Inter.Cubic);

        /// <summary>
        /// Normalise Image using min-max normalisation from EmguCV
        /// </summary>
        /// <param name="faceImage"></param>
        /// <returns>Emgu.CV Image object</returns>
        public static Image<Gray, byte> NormalizeImage(Image<Gray, byte> faceImage)
        {
            CvInvoke.Normalize(faceImage, faceImage, 0, 255, Emgu.CV.CvEnum.NormType.MinMax);
            return faceImage;
        }

        /// <summary>
        /// Apply equalisation histogram to image
        /// </summary>
        /// <param name="faceImage"></param>
        /// <returns>Emgu.CV Image object</returns>
        public static Image<Gray, byte> EqualizeImage(Image<Gray, byte> faceImage)
        {
            CvInvoke.EqualizeHist(faceImage, faceImage);
            return faceImage;
        }

        /// <summary>
        /// Apply Gaussian blur to image
        /// </summary>
        /// <param name="faceImage"></param>
        /// <returns>Emgu.CV Image object</returns>
        public static Image<Gray, byte> GaussianBlurImage(Image<Gray, byte> faceImage)
        {
            CvInvoke.GaussianBlur(faceImage, faceImage, new Size(3, 3), 0);
            return faceImage;
        }

        /// <summary>
        /// Apply standard image processing to an image.
        /// </summary>
        /// <param name="image"></param>
        /// <returns>Emgu.CV Image with applied filters</returns>
        public static Image<Gray, byte> ProcessImage(Image<Gray, byte> image)
        {
            image = ImageHandler.ResizeImage(image);
            image = ImageHandler.NormalizeImage(image);
            image = ImageHandler.EqualizeImage(image);
            image = ImageHandler.GaussianBlurImage(image);

            return image;
        }

    }
}
