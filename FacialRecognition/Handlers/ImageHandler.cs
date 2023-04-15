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

    }
}
