using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork1.SampleGeneration
{
    class AlphabetGenerator : IGenerator<AlphabetSampleData>
    {
        public int ClassesCount { get; set; } = 10;

        const string AlphabetPath = "..\\..\\Alphabet\\";

        private Bitmap currentBitmap;

        int IGenerator<AlphabetSampleData>.MaxClassesCount => Enum.GetValues(typeof(AlphabetLetter)).Length-1;

        private Dictionary<AlphabetLetter, List<Bitmap>> letters;

        public AlphabetGenerator()
        {
            
            letters = new Dictionary<AlphabetLetter, List<Bitmap>>();
            foreach(var letter in Enum.GetValues(typeof(AlphabetLetter)))
            {
                var alphabetLetter = (AlphabetLetter)letter;
                if (alphabetLetter == AlphabetLetter.Undef) continue;
                letters[alphabetLetter] = new List<Bitmap>();
                foreach(var path in Directory.GetFiles(AlphabetPath + alphabetLetter.ToString()))
                {
                    Bitmap bitmap = new Bitmap(path);
                    ResizeBilinear processedScaleFilter = new ResizeBilinear(200, 200);
                    bitmap = processedScaleFilter.Apply(bitmap);
                    letters[alphabetLetter].Add(bitmap);
                }
            }
        }

        public Bitmap GenerateBitmap()
        {
            return currentBitmap;
        }

  

        Sample<AlphabetSampleData> IGenerator<AlphabetSampleData>.GenerateFigure()
        {
            Random random = new Random();
            AlphabetLetter letter = (AlphabetLetter)random.Next(ClassesCount);
            var bitmapList = letters[letter];
            currentBitmap = bitmapList[random.Next(bitmapList.Count)];
            var result = new Sample<AlphabetSampleData>(currentBitmap.ToInput(), ClassesCount, new AlphabetSampleData(letter));
            return result;
        }
    }
}
