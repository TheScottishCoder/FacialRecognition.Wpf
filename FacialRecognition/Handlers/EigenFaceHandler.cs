using Emgu.CV;
using Emgu.CV.Face;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using FacialRecognition.Helper;
using FacialRecognition.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FacialRecognition.Handlers
{
    public class EigenFaceHandler
    {
        private static int _imageCount = 0;
        private static double _threshold = 7000;
        private static List<Image<Gray, byte>> _faces = new List<Image<Gray, byte>>();
        private static List<string> _names = new List<string>();
        private static List<long> _personsLabels = new List<long>();
        public static bool isTrained = false;
        public static EigenFaceRecognizer recognizer;

        public static List<Image<Gray, byte>> GetFaces() => _faces;
        public static List<string> GetNames() => _names;
        public static List<long> GetPersonLabels() => _personsLabels;

        public static void Train()
        {
            var people = PersonStorage.Instance.People;

            _faces.Clear();
            _personsLabels.Clear();
            _names.Clear();

            foreach(Person person in people)
            {              
                string name = person.Name;
                long id = person.Id;

                foreach(var img in person.Images)
                {
                    _faces.Add(img.Face);
                    _personsLabels.Add(id);
                    _names.Add(name);
                    _imageCount++;
                }

                if(_faces.Count > 0)
                {
                    recognizer = new EigenFaceRecognizer(_imageCount, _threshold);

                    Mat[] mats = _faces.Select(f => f.Mat).ToArray();
                    IInputArrayOfArrays inputMats = new VectorOfMat(mats);

                    Mat labelsMat = new Mat(_personsLabels.Count, 1, Emgu.CV.CvEnum.DepthType.Cv32S, 1);
                    labelsMat.SetTo(_personsLabels.ToArray());

                    recognizer.Train(inputMats, labelsMat);

                    isTrained = true;
                }
            }

        }
    }
}
