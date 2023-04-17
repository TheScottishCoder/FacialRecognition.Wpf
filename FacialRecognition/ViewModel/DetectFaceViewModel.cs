using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using FacialRecognition.Handlers;
using FacialRecognition.Helper;
using FacialRecognition.Model;
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
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Visibility EventHandler
        public static EventHandler? VisibilityUpdate;

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

        private ImageSource _lastRecognisedFace;
        public ImageSource LastRecognisedFace
        {
            get => _lastRecognisedFace;
            set
            {
                _lastRecognisedFace = value;
                OnPropertyChanged(nameof(LastRecognisedFace));
            }
        }

        private bool _trainBloat;
        public bool TrainBloat
        {
            get => _trainBloat;
            set
            {
                _trainBloat = value;
                OnPropertyChanged(nameof(TrainBloat));
            }
        }

        private bool _frameThrottle = true;
        public bool FrameThrottle
        {
            get => _frameThrottle;
            set
            {
                _frameThrottle = value;
                OnPropertyChanged(nameof(FrameThrottle));
            }
        }

        private bool _groupTrain = false;
        public bool GroupTrain
        {
            get => _groupTrain;
            set
            {
                _groupTrain = value;
                OnPropertyChanged(nameof(GroupTrain));
            }
        }

        private string _groupTrainSearch;
        public string GroupTrainSearch
        {
            get => _groupTrainSearch;
            set
            {
                _groupTrainSearch = value;
                OnPropertyChanged(nameof(GroupTrainSearch));
            }
        }

        private string _label;
        public string Label
        {
            get => "Label: " + _num;
            set { }
        }

        private int _num = -1;
        public int Num
        {
            get => _num;
            set
            {
                _num = value;
                OnPropertyChanged(nameof(Label));
            }
        }

        public string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        // Commands
        public RelayCommand EnableCameraCommand { get; }
        public RelayCommand TrainCommand { get; }
        public RelayCommand GoCommand { get; }

        // Camera
        private volatile bool _canCapture = false;

        // Inititate commands and subscribe to events
        public DetectFaceViewModel()
        {
            EnableCameraCommand = new RelayCommand(CameraEnabled);
            TrainCommand = new RelayCommand(Train);
            GoCommand = new RelayCommand(Go);

            VisibilityUpdate += VisibilityChange;
        }

        private void Go()
        {

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


                if (Num != result.Label)
                {
                    if(PersonDatabase.Context.People.Any(y => y.Id == result.Label))
                        PersonDatabase.Context.People.FirstOrDefault(x => x.Id == result.Label).RecognisedCounter++;

                    LastRecognisedFace = haarExtract.ToBitmapSource();
                }

                Num = result.Label;
                
                // Draw results
                DrawFaceResults(result, emguFrameImage, haarLocation);
            }

            // Update PersonDatabase logs


            return new Tuple<Image<Bgr, byte>, Image<Gray, byte>>(emguFrameImage, haarExtract);
        }

        // Uses System.Drawing library
        private void DrawFaceResults(PredictionResult result, Image<Bgr, byte> frame, Rectangle haarRectangle)
        {
            if (result.Label != -1 && result.Distance < 2000)
            {
                List<string> names = EigenFaceHandler.GetNames();

                Name = names[Num];

                // Draw Name text above the person
                CvInvoke.PutText(frame, names[result.Label], new System.Drawing.Point(haarRectangle.X - 2, haarRectangle.Y - 2),
                    Emgu.CV.CvEnum.FontFace.HersheyComplex, 1.0, new Bgr(System.Drawing.Color.Purple).MCvScalar);
                // Draw a rectangle around the person
                CvInvoke.Rectangle(frame, haarRectangle, new Bgr(System.Drawing.Color.Green).MCvScalar, 2);
            }
            else
            {
                Name = "Unknown";
                CvInvoke.PutText(frame, "Unknown", new System.Drawing.Point(haarRectangle.X - 2, haarRectangle.Y - 2),
                                    FontFace.HersheyComplex, 1.0, new Bgr(System.Drawing.Color.Orange).MCvScalar);
                CvInvoke.Rectangle(frame, haarRectangle, new Bgr(System.Drawing.Color.Red).MCvScalar, 2);
            }
        }

        // Train the EigenFaceRecognizer
        private void Train()
        {
            IReadOnlyList<PersonModel> personModels = PersonDatabase.Context.People;
            List<PersonModel> models = new List<PersonModel>();


            if (GroupTrain)
            {
                
            }
            else
            {
                foreach (var p in PersonDatabase.Context.People)
                {
                    models.Add(p);
                }

                EigenFaceHandler.Train(models, TrainBloat);
            }
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

                if(FrameThrottle)
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
