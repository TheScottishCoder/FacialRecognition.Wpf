using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FacialRecognition.Model
{
    public class Image
    {
        private Image<Gray, byte> _face;
        public Image<Gray, byte> Face 
        { 
            get => _face;
            set
            {
                _face = value;
                Source = _face.ToBitmapSource();
            }
        }
        public int Row { get; set; }
        public int Col { get; set; }
        public ImageSource Source { get; private set; }
    }
}
