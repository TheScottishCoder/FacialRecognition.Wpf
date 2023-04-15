using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacialRecognition.Wrappers
{
    public class VideoCaptureWrapper
    {
        public static VideoCapture Instance { get; private set; } = new VideoCapture(0);

        //public VideoCaptureWrapper(int device = 0)
        //{
        //    Instance = new VideoCapture(device);
        //}
    }
}
