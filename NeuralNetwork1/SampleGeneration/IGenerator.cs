using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork1
{
    public interface IGenerator<T> where T: ISampleData,  new()
    {
        int ClassesCount { get; set; }
        Sample<T> GenerateFigure();
        Bitmap GenerateBitmap();

        
    }
}
