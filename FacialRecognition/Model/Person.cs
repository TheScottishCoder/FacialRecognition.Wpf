using FacialRecognition.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Media.Imaging;

namespace FacialRecognition.Model
{
    public class Person
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<Model.Image> Images { get; set; } = new List<Model.Image>();
        public string Group { get; set; } // Class
        public float Timeout { get; set; }

        public Person(long id, string name, List<Model.Image> images, string group, float timeout)
        {
            this.Id = id;
            this.Name = name;
            this.Images = images;
            this.Group = group;
            this.Timeout = timeout;

            _timer = new Timer(timeout * 60000);
            _timer.Elapsed += SelfDestroy;
            _timer.Enabled = true;
        }

        // For destroying data after timeout period
        private static Timer _timer;

        private void SelfDestroy(Object source, ElapsedEventArgs e)
        {
            PersonStorage.Instance.RemovePerson(this);
        }
    }
}
