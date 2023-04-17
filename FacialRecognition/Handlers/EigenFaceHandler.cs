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
        public static void Train(List<PersonModel> people, bool bloat)
        {
            Reset();

            Preprocess(people, bloat);


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

        private static void Reset()
        {
            isTrained = false;
            _imageCount = 0;
            _faces.Clear();
            _personsLabels.Clear();
            _names.Clear();
        }

        public static int Preprocess(List<PersonModel> people, bool bloat)
        {
            int label = 0;

            // Loop each person in people list
            foreach (PersonModel person in people)
            {
                string name = person.Name;
                long id = person.Id;
                _names.Add(name);

                foreach (var img in person.Images)
                {
                    // Allways add image with full pre-processing
                    _faces.Add(ImageHandler.ProcessImage(img.Face));

                    // if bloat is true, generate extra training images (No effect, Normalized, Equalized, GaussianBlur) 
                    // Bloat Example; input Images = 3(1) -> bloat -> Result = 18(6) (3x Full processing, 3x Normalize, 3x Equalize, 3x Gaussian, 3x No Effect)
                    if (bloat)
                    {
                        _faces.Add(img.Face);
                        _faces.Add(ImageHandler.NormalizeImage(img.Face));
                        _faces.Add(ImageHandler.EqualizeImage(img.Face, 1));
                        _faces.Add(ImageHandler.EqualizeImage(img.Face, 2));
                        _faces.Add(ImageHandler.GaussianBlurImage(img.Face, new System.Drawing.Size(3, 3)));

                        for (int i = 0; i <= 4; i++)
                        {
                            _personsLabels.Add(label);
                            _imageCount++;
                        }
                    }

                    _personsLabels.Add(label);
                    _imageCount++;
                }

                label++;
            }

            return label;
        }

        public static int GetLabelFromPersonName(string personName)
        {
            int label = _names.IndexOf(personName);

            return label;
        }
    }
}