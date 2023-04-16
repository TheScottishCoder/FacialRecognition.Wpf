﻿using Accord.MachineLearning;
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
        // Haar Cascade
        private static CascadeClassifier _classifer;

        private static void CheckClassifer()
        {
            string faceCascadePath = Path.GetFullPath(@"data/haarcascades_frontalface_default.xml");

            if (_classifer == null)
                _classifer = new CascadeClassifier(faceCascadePath);
        }

        public static Image<Bgr, byte> HaarCascadeFaceDetect(BitmapImage image)
        {
            CheckClassifer();

            Image<Bgr, byte> img = ImageHandler.BitmapImageToEmguImage(image);
            var imgGray = img.Convert<Gray, byte>();

            // scaleFactor = Lower means more accurate results but longer processing times
            Rectangle[] faces = _classifer.DetectMultiScale(imgGray, 1.1, 4);

            foreach (var face in faces)
            {
                img.Draw(face, new Bgr(0, 0, 255), 2);
            }

            return img;
        }

        public static Image<Gray, byte> HaarCascadeFaceExtract(BitmapImage image)
        {
            CheckClassifer();

            Image<Bgr, byte> img = ImageHandler.BitmapImageToEmguImage(image);
            var imgGray = img.Convert<Gray, byte>();

            // scaleFactor = Lower means more accurate results but longer processing times
            Rectangle[] faces = _classifer.DetectMultiScale(imgGray, 1.1, 4);

            Rectangle largestFace = faces.OrderByDescending(f => f.Size.Width * f.Size.Height).FirstOrDefault();

            if(largestFace.X == 0 && largestFace.Y == 0) { return null; }

            var faceImage = imgGray.Copy(largestFace);
            faceImage = ImageHandler.ResizeImage(faceImage);
            
            return faceImage;
        }

        public static Rectangle HaarCascadeFaceRectangle(BitmapImage image)
        {
            CheckClassifer();

            Image<Bgr, byte> img = ImageHandler.BitmapImageToEmguImage(image);
            var imgGray = img.Convert<Gray, byte>();

            // scaleFactor = Lower means more accurate results but longer processing times
            Rectangle[] faces = _classifer.DetectMultiScale(imgGray, 1.1, 4);

            Rectangle largestFace = faces.OrderByDescending(f => f.Size.Width * f.Size.Height).FirstOrDefault();

            return largestFace;
        }

        private static Image<Gray, byte> ProcessImage(Image<Gray, byte> image)
        {
            image = ImageHandler.ResizeImage(image);
            ImageHandler.NormalizeImage(image);
            ImageHandler.EqualizeImage(image);

            return image;
        }
    }
}
