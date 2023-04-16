using FacialRecognition.Helper;
using FacialRecognition.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace FacialRecognition.ViewModel
{
    internal class FaceListViewModel : INotifyPropertyChanged
    {
        // INotifyPropertyChanged Event handler
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Visibility EventHandler
        public static EventHandler VisibilityUpdate;

        // Binding
        public ObservableCollection<PersonModel> people { get; private set; } = new ObservableCollection<PersonModel>();
        public ObservableCollection<Model.FaceModel> Images { get; } = new ObservableCollection<Model.FaceModel>();
        private PersonModel _selectedPerson;
        public PersonModel SelectedPerson
        {
            get => _selectedPerson;
            set
            {
                _selectedPerson = value;
                OnPropertyChanged(nameof(SelectedPerson));
                LoadImages();
            }
        }

        // Commands
        public RelayCommand LoadPeopleCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public FaceListViewModel()
        {
            LoadPeopleCommand = new RelayCommand(LoadPeople);
            DeleteCommand = new RelayCommand(DeletePerson);
        }

        public void LoadPeople()
        {
            var p = PersonDatabase.Context.People;
            people.Clear();

            foreach (var person in p)
            {
                people.Add(person);
            }
        }

        public void LoadImages()
        {
            if (SelectedPerson != null)
            {
                Images.Clear();

                foreach (var img in SelectedPerson.Images)
                {
                    Images.Add(img);
                }
            }
        }

        public void DeletePerson()
        {
            PersonDatabase.Context.RemovePerson(SelectedPerson);
            SelectedPerson = null;
            Images.Clear();
            LoadPeople();
        }

    }
}
