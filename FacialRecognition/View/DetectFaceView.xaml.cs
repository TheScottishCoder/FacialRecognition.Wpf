using FacialRecognition.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FacialRecognition.View
{
    public partial class DetectFaceView : UserControl
    {
        public DetectFaceView()
        {
            InitializeComponent();
            DataContext = new DetectFaceViewModel();
        }
    }
}
