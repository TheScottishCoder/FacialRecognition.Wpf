using Emgu.CV;
using Emgu.CV.Structure;
using FacialRecognition.Handlers;
using FacialRecognition.Helper;
using FacialRecognition.Model;
using FacialRecognition.Wrappers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FacialRecognition.ViewModel
{
    public class AddFaceViewModel : INotifyPropertyChanged
    {
        // INotifyPropertyChanged Event Handlers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Visibility Change
        public static EventHandler VisibilityUpdate;

        // Binding
        private BitmapImage _cameraSource;
        public BitmapImage CameraSource
        {
            get => _cameraSource;
            set
            {
                _cameraSource = value;
                OnPropertyChanged(nameof(CameraSource));
            }
        }

        private string _name = "Name here";
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private string _group = "Class here";
        public string Group
        {
            get => _group;
            set
            {
                _group = value;
                OnPropertyChanged(nameof(Group));
            }
        }

        private float _timeout = 5;
        public float Timeout
        {
            get => _timeout;
            set
            {
                _timeout = value;
                OnPropertyChanged(nameof(Timeout));
            }
        }

        // Binding list to data template
        public ObservableCollection<Model.Image> Images { get; } = new ObservableCollection<Model.Image>();

        // Commands
        public RelayCommand EnableCameraCommand { get; }
        public RelayCommand TakeImageCommand { get; }
        public RelayCommand ClearImagesCommand { get; }
        public RelayCommand AddPersonCommand { get; }

        // Camera 
        private volatile bool _canCapture = false;
        private int _cameraDevice = 0;

        // Box row and column
        private int _column;
        private int _row;
        private static int counter = 0;

        public AddFaceViewModel()
        {
            EnableCameraCommand = new RelayCommand(CameraEnabled);
            TakeImageCommand = new RelayCommand(TakeImage);
            ClearImagesCommand = new RelayCommand(ClearImages);
            AddPersonCommand = new RelayCommand(AddPerson);

            VisibilityUpdate += VisibilityChange;
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

        private async void RenderFrame()
        {
            while (_canCapture)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CameraSource = CaptureFrame();

                });

                await Task.Delay(30);
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

        // Add Gray Scale
        private void TakeImage()
        {
            if (_canCapture)
            {
                Image<Gray, Byte> img = ProcessImage(CaptureFrame());
                Images.Add(new Model.Image
                {
                    Face = img
                });
            }
        }

        private void ClearImages()
        {
            Images.Clear();
        }

        private void AddPerson()
        {
            PersonStorage.Instance.AddPerson(new Person(
                //GenerateRandomID(),
                counter++,
                this.Name,
                this.Images.ToList(),
                this.Group,
                this.Timeout
            ));

            Cleanup();
        }

        private long GenerateRandomID()
        {
            Random rnd = new Random();
            long rndId = rnd.NextInt64();

            if (CheckIdValid(rndId))
                return rndId;
            else
                return GenerateRandomID();
        }

        private bool CheckIdValid(long num) =>
            !PersonStorage.Instance.People.Any(x => x.Id == num);

        private void Cleanup()
        {
            Images.Clear();
            Name = "Name here";
            Group = "Class here";
            Timeout = 5;
        }

        private void VisibilityChange(object sender, EventArgs e)
        {
            _canCapture = false;
            VideoCaptureWrapper.Instance.Stop();
        }

        private Image<Gray, byte> ProcessImage(BitmapImage img)
        {
            var processedImage = HaarCascadeHandler.HaarCascadeFaceExtract(img);

            return processedImage;
        }
    }
}
