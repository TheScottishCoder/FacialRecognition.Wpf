using FacialRecognition.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;

namespace FacialRecognition.ViewModel
{
    public class MenuBarViewModel : INotifyPropertyChanged
    {
        // Relay Commands
        public RelayCommand ShowAddFaceViewCommand { get; }
        public RelayCommand ShowDetectFaceViewCommand { get; }
        public RelayCommand ShowFaceListViewCommand { get; }

        // Property Changed Event Handler
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Visibility binding
        private Visibility _addFaceWrapperVisibility;
        public Visibility AddFaceWrapperVisibility
        {
            get => _addFaceWrapperVisibility;
            set
            {
                _addFaceWrapperVisibility = value;
                OnPropertyChanged(nameof(AddFaceWrapperVisibility));
            }
        }

        private Visibility _detectFaceWrapperVisibility;
        public Visibility DetectFaceWrapperVisibility
        {
            get => _detectFaceWrapperVisibility;
            set
            {
                _detectFaceWrapperVisibility = value;
                OnPropertyChanged(nameof(DetectFaceWrapperVisibility));
            }
        }

        private Visibility _faceListWrapperVisibility;
        public Visibility FaceListWrapperVisibility
        {
            get => _faceListWrapperVisibility;
            set
            {
                _faceListWrapperVisibility = value;
                OnPropertyChanged(nameof(FaceListWrapperVisibility));
            }
        }

        public MenuBarViewModel()
        {
            ShowAddFaceViewCommand = new RelayCommand(ShowAddFaceView);
            ShowDetectFaceViewCommand = new RelayCommand(ShowDetectFaceView);
            ShowFaceListViewCommand = new RelayCommand(ShowFaceListView);

            CollapseAll();
        }

        private void CollapseAll()
        {
            AddFaceWrapperVisibility = Visibility.Collapsed;
            DetectFaceWrapperVisibility = Visibility.Collapsed;
            FaceListWrapperVisibility = Visibility.Collapsed;
        }

        private void ShowAddFaceView()
        {
            CollapseAll();
            //AddFaceViewModel.VisibilityUpdate.Invoke(this, null);
            AddFaceWrapperVisibility = Visibility.Visible;
        }

        private void ShowDetectFaceView()
        {
            CollapseAll();
            //DetectFaceViewModel.VisibilityUpdate.Invoke(this, null);
            DetectFaceWrapperVisibility = Visibility.Visible;
        }
        
        private void ShowFaceListView()
        {
            CollapseAll();
            //DetectFaceViewModel.VisibilityUpdate.Invoke(this, null);
            FaceListWrapperVisibility = Visibility.Visible;
        }
    }
}
