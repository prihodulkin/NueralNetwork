using FastBitmapLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork1
{
    static class Helpers
    {
        public static void MutliplyAndApplySigmoid(this double[] vector, double [,] matrix, double[] result)
        {
            var rowsCount = matrix.GetLength(0);
            var colCount = matrix.GetLength(1);
            for (int i = 0; i <colCount ; i++)
            {
                double sum = 0;
                for(int j = 0; j <rowsCount; j++)
                {
                    sum += vector[j] * matrix[j, i];
                }
                result[i] = sum.Sigmoid();
            }  
        }

        public static void MutliplyAndApplySigmoidParallel(this double[] vector, double[,] matrix, double[] result)
        {
            var rowsCount = matrix.GetLength(0);
            var colCount = matrix.GetLength(1);
            for (int i = 0; i < colCount; i++)
            {
                double sum = 0;
                Parallel.For(0, rowsCount, (j) => sum += vector[j] * matrix[j, i]);
                result[i] = sum.Sigmoid();
            }
        }

        public static double Sigmoid(this double value)
        {
            return 1.0 / (Math.Exp(-value) + 1);
        }

        public static double[] ToInput(this Bitmap bitmap)
        {
            double[] result = new double[bitmap.Width + bitmap.Height];
            //var fastBitmap = new FastBitmap(bitmap);
            //fastBitmap.Lock();
            //using (var fastBitmap = bitmap.FastLock())
            //{
            var arr = new Color[bitmap.Width, bitmap.Height];
                for (int x = 0; x < bitmap.Width; x++)
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        
                        var c = bitmap.GetPixel(x, y);
                        arr[x, y] = c;
                        if (c == Color.FromArgb(255,0,0,0))
                        {
                            result[x]++;
                            result[y + bitmap.Width]++;
                        }
                    }
                }
           // fastBitmap.Unlock();
           // }
            return result;
        }

    }
}
