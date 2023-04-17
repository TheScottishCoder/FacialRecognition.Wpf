using Accord;
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

        private bool _evaluate = false;
        public bool Evaluate
        {
            get => _evaluate;
            set
            {
                _evaluate = value;
                OnPropertyChanged(nameof(Evaluate));
            }
        }

        private string _evaluatePerson;
        public string EvaluatePerson
        {
            get => _evaluatePerson;
            set
            {
                _evaluatePerson = value;
                OnPropertyChanged(nameof(EvaluatePerson));
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

        // Eval stuff
        private int[,] confusionMatrix;
        private int numClasses;
        bool doEval = false;
        double overallResultAccuracy = 0;
        double overallAttemptAccuracy = 0;
        int predictionAttempts = 0;
        int timesPredicted = 0;

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
            if (confusionMatrix == null)
            {
                
            }

            CalculateAccuracy();
        }

        // Bulk processing for Detection, Extraction and Drawing
        private Tuple<Image<Bgr, byte>, Image<Gray, byte>, PredictionResult> Process(bool eval = true)
        {
            // Eval stats
            int truePositives = 0;
            int falsePositives = 0;

            int trueNegatives = 0;
            int falseNegatives = 0;

            // Capture Frames, Extracted Face and Location. 
            var frame = CaptureFrame();
            var haarExtract = HaarCascadeHandler.HaarCascadeFaceExtract(frame);
            var haarLocation = HaarCascadeHandler.HaarCascadeFaceRectangle(frame);
            var emguFrameImage = ImageHandler.BitmapImageToEmguImage(frame);

            PredictionResult result = default;

            // if anything is null stop
            if(haarExtract == null || (haarLocation.X == 0 && haarLocation.Y == 0)) { return null; }

            // if the recognizer is trained recognize images
            if (EigenFaceHandler.isTrained)
            {
                haarExtract = ImageHandler.ProcessImage(haarExtract);

                // Predict and store results of recognition
                result = EigenFaceHandler.recognizer.Predict(haarExtract);


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


            return new Tuple<Image<Bgr, byte>, Image<Gray, byte>, PredictionResult>(emguFrameImage, haarExtract, result);
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



                foreach (var p in PersonDatabase.Context.People)
                {
                    models.Add(p);
                }

                EigenFaceHandler.Train(models, TrainBloat);

            numClasses = PersonDatabase.Context.People.Count;
            confusionMatrix = new int[numClasses, numClasses];
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
                    Tuple<Image<Bgr, byte>, Image<Gray, byte>, PredictionResult> img = null;

                    if (Evaluate == false)
                    {
                        img = Process();                        
                    }
                    else if(Evaluate == true)
                    {          
                        img = Process();

                        Evaluation(img);
                    }

                    if (img != null)
                    {
                        if (img.Item1 != null)
                            CameraSource = img.Item1.ToBitmapSource();
                        if (img.Item2 != null)
                            ExtractedFace = img.Item2.ToBitmapSource();
                    }
                });

                if (FrameThrottle)
                    await Task.Delay(200);
                else
                    await Task.Delay(30);
            }
        }

        private void Evaluation(Tuple<Image<Bgr, byte>, Image<Gray, byte>, PredictionResult> img)
        {
            if (img == null)
            {
                return;
            }

            if (img.Item3.Label == -1)
            {
                predictionAttempts++;
                return;
            }

            if (EvaluatePerson == null) return;

            confusionMatrix[EigenFaceHandler.GetLabelFromPersonName(EvaluatePerson), img.Item3.Label]++;
            timesPredicted++;
        }

        private void CalculateAccuracy()
        {
            int correctPredictions = 0;
            int totalPredictions = 0;

            for (int i = 0; i < numClasses; i++)
            {
                for (int j = 0; j < numClasses; j++)
                {
                    totalPredictions += confusionMatrix[i, j];
                    if (i == j)
                    {
                        correctPredictions += confusionMatrix[i, j];
                    }
                }
            }

            overallResultAccuracy = (double)correctPredictions / totalPredictions;
            overallAttemptAccuracy = (double)timesPredicted / predictionAttempts;

            List<double> data = new List<double>() { (double)totalPredictions , (double)correctPredictions, (double)predictionAttempts,
                (double)timesPredicted, overallResultAccuracy, overallAttemptAccuracy
            };
            List<string> tags = new List<string>() { "Total Predictions", "Correct Predictions", "Prediction Attempts",
                "Times Predicted", "Overall Result Accuracy", "overallAttemptAccuracy" };

            WriteDataToFile(data, tags, $"output{EvaluatePerson}.txt");
        }

        public static void WriteDataToFile(List<double> data, List<string> tags, string fileName)
        {
            using (StreamWriter outputFile = new StreamWriter(fileName))
            {
                for (int i = 0; i < data.Count; i++)
                {
                    outputFile.WriteLine($"{tags[i]}: {data[i]}");
                }
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
