using Accord.MachineLearning;
using Emgu.CV.Structure;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing;

namespace FacialRecognition.Handlers
{
    public class HaarCascadeHandler
    {
        // static CascadeClassifier Object
        private static CascadeClassifier? _classifer;

        /// <summary>
        /// Check for valid CascadeClassifier or create one.
        /// </summary>
        private static void CheckClassifer()
        {
            string faceCascadePath = Path.GetFullPath(@"data/haarcascades_frontalface_default.xml");

            if (_classifer == null)
                _classifer = new CascadeClassifier(faceCascadePath);
        }

        /// <summary>
        /// Uses Haar's Cascade to detect and extract a face in an image. May return null.
        /// </summary>
        /// <param name="image"></param>
        /// <returns>Emgu.CV Gray Image object.</returns>
        public static Image<Gray, byte> HaarCascadeFaceExtract(BitmapImage image)
        {
            CheckClassifer();

            // Convert to image to EmguImage and then converts to grey scale.         
            Image<Gray,byte> imgGray = ImageHandler.BitmapImageToEmguImage(image)
                .Convert<Gray, byte>();

            // Use the classifier to detect faces
            // scaleFactor = Lower means more accurate results but longer processing times
            Rectangle[] faces = _classifer.DetectMultiScale(imgGray, 1.1, 4);

            // Get's the largest recetangle
            Rectangle largestFace = faces.OrderByDescending(f => f.Size.Width * f.Size.Height).FirstOrDefault();

            // Empty rectangles are often at 0,0
            if(largestFace.X == 0 && largestFace.Y == 0) { return null; }

            // Resize image
            var faceImage = imgGray.Copy(largestFace);
            faceImage = ImageHandler.ResizeImage(faceImage);
            
            return faceImage;
        }

        /// <summary>
        /// Gets the rectangle object produced by HaarCascade containing the location of detected face
        /// </summary>
        /// <param name="image"></param>
        /// <returns>System.Drawing.Rectangle object of the largest face</returns>
        public static Rectangle HaarCascadeFaceRectangle(BitmapImage image)
        {
            CheckClassifer();

            // Convert to image to EmguImage and then converts to grey scale.         
            Image<Gray, byte> imgGray = ImageHandler.BitmapImageToEmguImage(image)
                .Convert<Gray, byte>();

            // Use the classifier to detect faces
            // scaleFactor = Lower means more accurate results but longer processing times
            Rectangle[] faces = _classifer.DetectMultiScale(imgGray, 1.1, 4);

            // Get's the largest recetangle
            Rectangle largestFace = faces.OrderByDescending(f => f.Size.Width * f.Size.Height).FirstOrDefault();

            return largestFace;
        }
    }
}
