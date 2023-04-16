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
    public class PersonModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<Model.FaceModel> Images { get; set; } = new List<Model.FaceModel>();
        public string Group { get; set; } // Class
        public int RecognisedCounter { get; set; }
        public List<DateTime> RecognisedTime { get; set; }
        public float Timeout { get; set; }
        public string TimeRemaining
        {
            get
            {
                if (_timer == null) return "NULL VALUE";

                var elapsed = DateTime.Now - _startTime;
                var timeLeft = TimeSpan.FromMilliseconds((Timeout*60000)) - elapsed;

                if (timeLeft.TotalSeconds <= 0) return "00:00";

                return $"{timeLeft:mm\\:ss}";
            }
        }
        

        private static Timer? _timer;
        private DateTime _startTime;

        /// <summary>
        /// Initiates the person object and starts timer
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="images"></param>
        /// <param name="group"></param>sssss
        /// <param name="timeout"></param>
        public PersonModel(long id, string name, List<Model.FaceModel> images, string group, float timeout)
        {
            this.Id = id;
            this.Name = name;
            this.Images = images;
            this.Group = group;
            this.Timeout = timeout;

            _startTime = DateTime.Now;
            _timer = new Timer(timeout * 60000);
            _timer.Elapsed += SelfDestroy;
            _timer.Enabled = true;
        }

        /// <summary>
        /// Event triggers when timer is complete. This will destroy the objects data.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void SelfDestroy(Object? source, ElapsedEventArgs e)
        {
            PersonDatabase.Context.RemovePerson(this);
        }
    }
}
