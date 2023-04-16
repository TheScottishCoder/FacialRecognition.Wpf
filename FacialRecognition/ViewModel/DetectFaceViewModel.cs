using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using FacialRecognition.Handlers;
using FacialRecognition.Helper;
using FacialRecognition.Wrappers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using static Emgu.CV.Face.FaceRecognizer;

namespace FacialRecognition.ViewModel
{
    public class DetectFaceViewModel : INotifyPropertyChanged
    {
        // INotifyPropertyChanged Event handler
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Visibility EventHandler
        public static EventHandler VisibilityUpdate;

        // Binding
        private ImageSource _cameraSource;
        public ImageSource CameraSource
        {
            get => _cameraSource;
            set
            {
                _cameraSource = value;
                OnPropertyChanged(nameof(CameraSource));
            }
        }

        private ImageSource _extractedFace;
        public ImageSource ExtractedFace
        {
            get => _extractedFace;
            set
            {
                _extractedFace = value;
                OnPropertyChanged(nameof(ExtractedFace));
            }
        }

        private int _num;
        public int Num
        {
            get => _num;
            set
            {
                _num = value;
                OnPropertyChanged(nameof(Num));
            }
        }

        // Commands
        public RelayCommand EnableCameraCommand { get; }
        public RelayCommand TrainCommand { get; }

        // Camera
        private volatile bool _canCapture = false;
        private int _cameraDevice = 0;

        // Features
        private bool _canDetect = true;

        // Inititate commands and subscribe to events
        public DetectFaceViewModel()
        {
            EnableCameraCommand = new RelayCommand(CameraEnabled);
            TrainCommand = new RelayCommand(Train);

            VisibilityUpdate += VisibilityChange;
        }

        // Bulk processing for Detection, Extraction and Drawing
        private Tuple<Image<Bgr, byte>, Image<Gray, byte>> Process()
        {
            // Capture Frames, Extracted Face and Location. 
            var frame = CaptureFrame();
            var haarExtract = HaarCascadeHandler.HaarCascadeFaceExtract(frame);
            var haarLocation = HaarCascadeHandler.HaarCascadeFaceRectangle(frame);
            var emguFrameImage = ImageHandler.BitmapImageToEmguImage(frame);

            // if anything is null stop
            if(haarExtract == null || (haarLocation.X == 0 && haarLocation.Y == 0)) { return null; }

            // if the recognizer is trained recognize images
            if (EigenFaceHandler.isTrained)
            {
                haarExtract = ImageHandler.ProcessImage(haarExtract);

                // Predict and store results of recognition
                PredictionResult result = EigenFaceHandler.recognizer.Predict(haarExtract);

                // Draw results
                DrawFaceResults(result, emguFrameImage, haarLocation);
                Num = result.Label;
            }

            return new Tuple<Image<Bgr, byte>, Image<Gray, byte>>(emguFrameImage, haarExtract);
        }

        // Uses System.Drawing library
        private void DrawFaceResults(PredictionResult result, Image<Bgr, byte> frame, Rectangle haarRectangle)
        {
            if (result.Label != -1 && result.Distance < 2000)
            {
                var names = EigenFaceHandler.GetNames();

                // Draw Name text above the person
                CvInvoke.PutText(frame, names[result.Label], new System.Drawing.Point(haarRectangle.X - 2, haarRectangle.Y - 2),
                    Emgu.CV.CvEnum.FontFace.HersheyComplex, 1.0, new Bgr(System.Drawing.Color.Purple).MCvScalar);
                // Draw a rectangle around the person
                CvInvoke.Rectangle(frame, haarRectangle, new Bgr(System.Drawing.Color.Green).MCvScalar, 2);
            }
            else
            {
                CvInvoke.PutText(frame, "Unknown", new System.Drawing.Point(haarRectangle.X - 2, haarRectangle.Y - 2),
                                    FontFace.HersheyComplex, 1.0, new Bgr(System.Drawing.Color.Orange).MCvScalar);
                CvInvoke.Rectangle(frame, haarRectangle, new Bgr(System.Drawing.Color.Red).MCvScalar, 2);
            }
        }

        // Train the EigenFaceRecognizer
        private void Train()
        {
            EigenFaceHandler.Train();
        }

        // Enable the camera
        private void CameraEnabled()
        {
            if (_canCapture == false)
            {
                _canCapture = true;
                VideoCaptureWrapper.Instance.Start();
                // Start RenderFrame on a new thread.
                Task.Run(() => RenderFrame());
            }
            else
            {
                _canCapture = false;
                //VideoCaptureWrapper.Instance.Stop();
            }
        }

        
        // Render a captured frame - Runs on a seperate thread.
        private async void RenderFrame()
        {
            while (_canCapture)
            {
                // Invokes the UI to uppdate on a seperate thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var img = Process();

                    if (img != null)
                    {
                        if (img.Item1 != null)
                            CameraSource = img.Item1.ToBitmapSource();
                        if (img.Item2 != null)
                            ExtractedFace = img.Item2.ToBitmapSource();
                    }
                });

                await Task.Delay(30);
            }
        }

        // capture a frame
        private BitmapImage CaptureFrame()
        {
            var bi = new BitmapImage();
            Bitmap? image = null;

            lock (VideoCaptureWrapper.Instance)
            {
                image = VideoCaptureWrapper.Instance.QueryFrame()?.ToBitmap();
                if (image != null)
                {
                    // Convert from BitmapImage to bitmap
                    var ms = new MemoryStream();
                    image.Save(ms, ImageFormat.Bmp);

                    ms.Seek(0, SeekOrigin.Begin);
                    bi.BeginInit();
                    bi.StreamSource = ms;
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.EndInit();
                    bi.Freeze();
                }
            }

            return bi;
        }

        // if visibility changes clean  some stuff
        private void VisibilityChange(object sender, EventArgs e)
        {
            _canCapture = false;
            //VideoCaptureWrapper.Instance.Stop();
        }
    }
}
