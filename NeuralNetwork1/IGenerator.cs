using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork1
{
    interface IGenerator
    {
        int ClassesCount { get; set; }
        Sample GenerateFigure();
        Bitmap GenerateBitmap();
    }
}
