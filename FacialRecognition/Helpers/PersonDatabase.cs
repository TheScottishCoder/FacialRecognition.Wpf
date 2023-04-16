using Emgu.CV;
using Emgu.CV.Structure;
using FacialRecognition.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FacialRecognition.Helper
{
    public class PersonDatabase
    {
        // This class is designed to replicate a database system
        // Data will not be permanently stored to disk as this adds privacy and personal data complications

        /// <summary>
        /// Create static Context of the database
        /// </summary>
        public static PersonDatabase Context { get; } = new PersonDatabase();

        /// <summary>
        /// Store Model.Person object
        /// </summary>
        private List<Model.PersonModel> _people;

        /// <summary>
        /// Initiate objects in constructor
        /// </summary>
        private PersonDatabase() =>
            _people = new List<Model.PersonModel>();

        /// <summary>
        /// Returns a read only list of stored Persons
        /// </summary>
        public IReadOnlyList<PersonModel> People => _people.AsReadOnly();

        /// <summary>
        /// Add Person to Database
        /// </summary>
        /// <param name="person"></param>
        public void AddPerson(PersonModel person) =>
            _people.Add(person);

        /// <summary>
        /// Add list of Person objects to Database
        /// </summary>
        /// <param name="people"></param>
        public void AddPeople(List<PersonModel> people) =>
            _people.AddRange(people);

        /// <summary>
        /// Remove person object from database
        /// </summary>
        /// <param name="person"></param>
        public void RemovePerson(PersonModel person) =>
            _people.Remove(person);
    }
}
