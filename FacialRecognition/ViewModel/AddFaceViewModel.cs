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
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Visibility Change
        public static EventHandler? VisibilityUpdate;

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
        public ObservableCollection<Model.FaceModel> Images { get; } = new ObservableCollection<Model.FaceModel>();

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

        // Inititate Commands with their functions and subscribe to event
        public AddFaceViewModel()
        {
            EnableCameraCommand = new RelayCommand(CameraEnabled);
            TakeImageCommand = new RelayCommand(TakeImage);
            ClearImagesCommand = new RelayCommand(ClearImages);
            AddPersonCommand = new RelayCommand(AddPerson);

            VisibilityUpdate += VisibilityChange;
        }

        // Enable the Camera
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

        // Capture a frame and render it - Runs on it's own thread
        private async void RenderFrame()
        {
            while (_canCapture)
            {
                // This will invoke the UI Updater to update the UI on a seperate thread once changes are made
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CameraSource = CaptureFrame();

                });

                await Task.Delay(30);
            }
        }

        // Capture a frame from the device
        private BitmapImage CaptureFrame()
        {
            var bi = new BitmapImage();
            Bitmap? image = null;

            // Lock the instance since it's a static object. Any thread could be trying to access it.
            lock (VideoCaptureWrapper.Instance)
            {
                // Capture the frame
                image = VideoCaptureWrapper.Instance.QueryFrame()?.ToBitmap();
                if (image != null)
                {
                    // Converts to BitmapImage from bitmap
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

        // Command triggered by button to take an image
        private void TakeImage()
        {
            if (_canCapture)
            {
                Image<Gray, Byte> img = ExtractFaceFromImage(CaptureFrame());

                if (img == null) { TakeImage(); }
                else
                {

                    Images.Add(new Model.FaceModel
                    {
                        Face = img
                    });
                }
            }
        }

        // Clear all images button
        private void ClearImages()
        {
            Images.Clear();
        }

        // Add person to the Database context
        private void AddPerson()
        {
            PersonDatabase.Context.AddPerson(new PersonModel(
                //GenerateRandomID(),
                counter++,
                this.Name,
                this.Images.ToList(),
                this.Group,
                this.Timeout
            ));

            Cleanup();
        }

        private void Cleanup()
        {
            Images.Clear();
            Name = "Name here";
            Group = "Class here";
            Timeout = 5;
        }

        // if the visibility event is triggered do some cleanup
        // this needs further looking at
        private void VisibilityChange(object sender, EventArgs e)
        {
            _canCapture = false;
            VideoCaptureWrapper.Instance.Stop();
        }

        // Take an image, extract it, check it's not null and return it.
        private Image<Gray, byte> ExtractFaceFromImage(BitmapImage img)
        {
            var processedImage = HaarCascadeHandler.HaarCascadeFaceExtract(img);
            if(processedImage == null) { return null; }

            return processedImage;
        }
    }
}
