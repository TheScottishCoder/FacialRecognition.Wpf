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

        public DetectFaceViewModel()
        {
            EnableCameraCommand = new RelayCommand(CameraEnabled);
            TrainCommand = new RelayCommand(Train);

            VisibilityUpdate += VisibilityChange;
        }

        private Tuple<Image<Bgr, byte>, Image<Gray, byte>> Process()
        {
            var frame = CaptureFrame();
            var haarResult = HaarCascadeHandler.HaarCascadeFaceDetect(frame);
            var haarExtract = HaarCascadeHandler.HaarCascadeFaceExtract(frame);
            var haarLocation = HaarCascadeHandler.HaarCascadeFaceRectangle(frame);
            var emguFrameImage = ImageHandler.BitmapImageToEmguImage(frame);

            if(haarResult == null || haarExtract == null || (haarLocation.X == 0 && haarLocation.Y == 0)) { return null; }

            var faceImage = ImageHandler.BitmapImageToEmguImage(frame).Copy(haarLocation);

            if (EigenFaceHandler.isTrained)
            {
                var names = EigenFaceHandler.GetNames();
                haarExtract = ImageHandler.NormalizeImage(ImageHandler.EqualizeImage(haarExtract));

                var result = EigenFaceHandler.recognizer.Predict(haarExtract);

                Num = result.Label;

                if(result.Label != -1 && result.Distance < 2000)
                {

                    CvInvoke.PutText(emguFrameImage, names[result.Label], new System.Drawing.Point(haarLocation.X - 2, haarLocation.Y - 2),
                        Emgu.CV.CvEnum.FontFace.HersheyComplex, 1.0, new Bgr(System.Drawing.Color.Purple).MCvScalar);
                    CvInvoke.Rectangle(emguFrameImage, haarLocation, new Bgr(System.Drawing.Color.Green).MCvScalar, 2);
                }
                else
                {
                    CvInvoke.PutText(emguFrameImage, "Unknown", new System.Drawing.Point(haarLocation.X - 2, haarLocation.Y - 2),
                                        FontFace.HersheyComplex, 1.0, new Bgr(System.Drawing.Color.Orange).MCvScalar);
                    CvInvoke.Rectangle(emguFrameImage, haarLocation, new Bgr(System.Drawing.Color.Red).MCvScalar, 2);
                }
            }

            return new Tuple<Image<Bgr, byte>, Image<Gray, byte>>(emguFrameImage, haarExtract);
        }

        private void Train()
        {
            EigenFaceHandler.Train();
        }

        private void CameraEnabled()
        {
            if (_canCapture == false)
            {
                _canCapture = true;
                VideoCaptureWrapper.Instance.Start();
                Task.Run(() => RenderFrame());
            }
            else
            {
                _canCapture = false;
                //VideoCaptureWrapper.Instance.Stop();
            }
        }

        List<Tuple<int, double>> tuples = new List<Tuple<int, double>>();
        private async void RenderFrame()
        {
            while (_canCapture)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    //CameraSource = HaarCascadeHandler.HaarCascadeFaceDetect(CaptureFrame()).ToBitmapSource();
                    //if (EigenFaceHandler.isTrained)
                    //{
                    //    var result = EigenFaceHandler.recognizer.Predict(HaarCascadeHandler.HaarCascadeFaceExtract(CaptureFrame()));
                    //    Console.WriteLine(result.Label + " : " + result.Distance);
                    //    tuples.Add(new Tuple<int, double>(result.Label, result.Distance));
                    //
                    //    if(tuples.Count >= 1)
                    //    {
                    //        Console.WriteLine("");
                    //    }
                    //}
                    var img = Process();

                    if (img != null)
                    {
                        if (img.Item1 != null)
                            CameraSource = img.Item1.ToBitmapSource();
                        if (img.Item2 != null)
                            ExtractedFace = img.Item2.ToBitmapSource();
                    }

                });

                await Task.Delay(10);
            }
        }

        private BitmapImage CaptureFrame()
        {
            var bi = new BitmapImage();
            Bitmap? image = null;

            lock (VideoCaptureWrapper.Instance)
            {
                image = VideoCaptureWrapper.Instance.QueryFrame()?.ToBitmap();
                if (image != null)
                {

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

        private void Cleanup(object sender, EventArgs e)
        {
        }

        private void VisibilityChange(object sender, EventArgs e)
        {
            _canCapture = false;
            //VideoCaptureWrapper.Instance.Stop();
        }










































    }
}
