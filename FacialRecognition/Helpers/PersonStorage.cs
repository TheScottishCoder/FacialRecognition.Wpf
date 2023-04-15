﻿using Emgu.CV;
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
    public class PersonStorage
    {
        // This class is designed to replicate a storage system
        // Data will not be permanently stored to disk as this adds privacy and personal data complications

        public static PersonStorage Instance { get; } = new PersonStorage();

        private List<Model.Person> _people;

        private PersonStorage() =>
            _people = new List<Model.Person>();

        public IReadOnlyList<Person> People => _people.AsReadOnly();

        public void AddPerson(Person person) =>
            _people.Add(person);


        public void AddPeople(List<Person> people) =>
            _people.AddRange(people);

        public void RemovePerson(Person person) =>
            _people.Remove(person);

        public List<Image<Gray, Byte>> GetImages()
        {
            List<Image<Gray, Byte>> sources = new List<Image<Gray, Byte>>();

            foreach(var p in _people)
            {
                foreach(var f in p.Images)
                {
                    sources.Add(f.Face);
                }
            }

            return sources;
        }
    }
}