using Emgu.CV;
using Emgu.CV.Structure;
using FacialRecognition.Helper;
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

        // Commands
        public RelayCommand EnableCameraCommand { get; }

        // Camera
        private volatile bool _canCapture = false;
        private int _cameraDevice = 0;

        // Features
        private bool _canDetect = true;

        public DetectFaceViewModel()
        {
            EnableCameraCommand = new RelayCommand(CameraEnabled);

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
                    CameraSource = ImageHandler.HaarCascadeFaceDetect(CaptureFrame()).ToBitmapSource();

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
