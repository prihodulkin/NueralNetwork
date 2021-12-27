using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork1
{
    /// <summary>
    /// генератор сэмплов для нейросети
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IGenerator<T> where T: ISampleData,  new()
    {
        /// <summary>
        /// количество классов, нужно для определения стуктуры нейросети
        /// </summary>
        int ClassesCount { get; set; }

        int MaxClassesCount { get; }

        /// <summary>
        /// генерация фигуры для обработки нейросетью
        /// </summary>
        /// <returns></returns>
        Sample<T> GenerateFigure();

        /// <summary>
        /// генерация битмапа для отображения в форме
        /// </summary>
        /// <returns></returns>
        Bitmap GenerateBitmap();

    }
}
