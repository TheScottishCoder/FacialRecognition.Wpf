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
        public ImageSource Source { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
    }
}
