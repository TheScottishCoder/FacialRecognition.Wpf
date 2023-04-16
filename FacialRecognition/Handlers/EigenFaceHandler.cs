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
using System.IO;

namespace FacialRecognition.Handlers
{
    public class EigenFaceHandler
    {
        // Private Fields
        private static int _imageCount = 0;
        private static double _threshold = 1000;

        private static List<Image<Gray, byte>> _faces = new List<Image<Gray, byte>>();
        private static List<string> _names = new List<string>();
        private static List<int> _personsLabels = new List<int>();

        // Public Fields
        public static EigenFaceRecognizer recognizer;
        public static bool isTrained = false;

        /// <summary>
        /// Get's the list of all trained faces
        /// </summary>
        /// <returns>A List of EmguCV Gray Images</returns>
        public static List<Image<Gray, byte>> GetFaces() => _faces;

        /// <summary>
        /// Get's the list of all names. 
        /// </summary>
        /// <returns>A List of Strings</returns>
        public static List<string> GetNames() => _names;

        /// <summary>
        /// Get's the list of labels
        /// </summary>
        /// <returns>A list of integers</returns>
        public static List<int> GetLabels() => _personsLabels;

        /// <summary>
        /// Trains the EigenFaceRecognizer with PersonStorage images.
        /// </summary>
        public static void Train()
        {
            _faces.Clear();
            _personsLabels.Clear();
            _names.Clear();

            IReadOnlyList<PersonModel> people = PersonDatabase.Context.People;
            int label = 0;

            // Loop each person in people list
            foreach (PersonModel person in people)
            {
                string name = person.Name;
                long id = person.Id;
                _names.Add(name);

                foreach (var img in person.Images)
                {
                    // Add different versions of the face image to the list
                    _faces.Add(img.Face);
                    _faces.Add(ImageHandler.NormalizeImage(img.Face));
                    _faces.Add(ImageHandler.EqualizeImage(img.Face));
                    _faces.Add(ImageHandler.GaussianBlurImage(img.Face));
                    _faces.Add(ImageHandler.ProcessImage(img.Face));

                    // Clean this up
                    _personsLabels.Add(label);
                    _personsLabels.Add(label);
                    _personsLabels.Add(label);
                    _personsLabels.Add(label);
                    _personsLabels.Add(label);
                    _imageCount++;
                    _imageCount++;
                    _imageCount++;
                    _imageCount++;
                    _imageCount++;
                }

                label++;
            }

            // create the output directory if it does not exist
            Directory.CreateDirectory("output");

            // loop through each image in _faces and save it to the output directory
            for (int i = 0; i < _faces.Count; i++)
            {
                string filename = Path.Combine("output", $"face_{i}.jpg");
                CvInvoke.Imwrite(filename, _faces[i]);
            }

            // if there is faces train
            if (_faces.Count > 0)
            {
                // Init recognizer with image count and threshold
                recognizer = new EigenFaceRecognizer(_imageCount, _threshold);

                // Convert images to Emgu.CV.Mat and create an IInputArrayOfArrays object
                Mat[] mats = _faces.Select(f => f.Mat).ToArray();
                IInputArrayOfArrays inputMats = new VectorOfMat(mats);

                // Convert the list of labels to a Emgu.CV Mat. 
                // Depth.Type must be same as the _personsLabel type
                // Incorrect type will default all values to 0
                Mat labelsMat = new Mat(_personsLabels.Count, 1, Emgu.CV.CvEnum.DepthType.Cv32S, 1);
                labelsMat.SetTo(_personsLabels.ToArray());

                // Train the recognizer
                recognizer.Train(inputMats, labelsMat);

                isTrained = true;
            }
        }
    }
}