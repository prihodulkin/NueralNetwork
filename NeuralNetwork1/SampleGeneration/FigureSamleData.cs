using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork1.SampleGeneration
{
    /// <summary>
    /// Тип фигуры
    /// </summary>
    public enum FigureType : byte { Triangle = 0, Rectangle, Circle, Sinusiod, Undef };

    public class FigureSampleData : ISampleData
    {
        public FigureType FigureType { get; private set; }


        /// <summary>
        /// нужен для удовлетоворения требования, которое не получилось внести в интерфейс
        /// </summary>
        public FigureSampleData() { }

        public FigureSampleData(FigureType figureType)
        {
            FigureType = figureType;
        }

        public void ByInt(int i)
        {
            FigureType = (FigureType)i;
        }

        public bool IsUndefined()
        {
            return FigureType == FigureType.Undef;
        }

        public int ToInt()
        {
            return (int)FigureType;
        }

        public bool Equals(ISampleData other)
        {
            if (other is FigureSampleData)
            {
                return FigureType == (other as FigureSampleData).FigureType;
            }

            return false;
        }

        public override string ToString()
        {
            return FigureType.ToString();
        }
    }
}
