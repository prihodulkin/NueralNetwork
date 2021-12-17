using System;
using System.Collections.Generic;
using System.Collections;

namespace NeuralNetwork1
{
    /// <summary>
    /// Класс для хранения образа – входной массив сигналов на сенсорах, выходные сигналы сети, и прочее
    /// </summary>
    public class Sample<T> where T : ISampleData, new()
    {
        /// <summary>
        /// Входной вектор
        /// </summary>
    public double[] input = null;

        /// <summary>
        /// Вектор ошибки, вычисляется по какой-нибудь хитрой формуле
        /// </summary>
        public double[] error = null;

        /// <summary>
        /// Действительный класс образа. Указывается учителем
        /// </summary>
        public T actualClass;

        /// <summary>
        /// Распознанный класс - определяется после обработки
        /// </summary>
        public T recognizedClass;

        /// <summary>
        /// Конструктор образа - на основе входных данных для сенсоров, при этом можно указать класс образа, или не указывать
        /// </summary>
        /// <param name="inputValues"></param>
        /// <param name="sampleClass"></param>
        public Sample(double[] inputValues, int classesCount, T sampleClass)
        {
            //  Клонируем массивчик
            input = (double[]) inputValues.Clone();
            Output = new double[classesCount];
            if (!sampleClass.IsUndefined()&&Output.Length>0) Output[sampleClass.ToInt()] = 1;


            recognizedClass = new T();
            actualClass = sampleClass;
        }

        /// <summary>
        /// Выходной вектор, задаётся извне как результат распознавания
        /// </summary>
        public double[] Output { get; private set; }

        /// <summary>
        /// Обработка реакции сети на данный образ на основе вектора выходов сети
        /// </summary>
        public T ProcessPrediction(double[] neuralOutput)
        {
            Output = neuralOutput;
            if (error == null)
                error = new double[Output.Length];

            //  Нам так-то выход не нужен, нужна ошибка и определённый класс
            recognizedClass = new T();
            recognizedClass.ByInt(0);
            for (int i = 0; i < Output.Length; ++i)
            {
                error[i] = (Output[i] - (i == actualClass.ToInt() ? 1 : 0));
                if (Output[i] > Output[recognizedClass.ToInt()])
                { 
                    recognizedClass.ByInt(i);
                }
            }

            return recognizedClass;
        }

        /// <summary>
        /// Вычисленная суммарная квадратичная ошибка сети. Предполагается, что целевые выходы - 1 для верного, и 0 для остальных
        /// </summary>
        /// <returns></returns>
        public double EstimatedError()
        {
            double Result = 0;
            for (int i = 0; i < Output.Length; ++i)
                Result += Math.Pow(error[i], 2);
            return Result;
        }

        /// <summary>
        /// Добавляет к аргументу ошибку, соответствующую данному образу (не квадратичную!!!)
        /// </summary>
        /// <param name="errorVector"></param>
        /// <returns></returns>
        public void updateErrorVector(double[] errorVector)
        {
            for (int i = 0; i < errorVector.Length; ++i)
                errorVector[i] += error[i];
        }

        /// <summary>
        /// Представление в виде строки
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string result = "Sample decoding : " + actualClass.ToString() + "(" + ( actualClass.ToInt()).ToString() +
                            "); " + Environment.NewLine + "Input : ";
            for (int i = 0; i < input.Length; ++i) result += input[i].ToString() + "; ";
            result += Environment.NewLine + "Output : ";
            if (Output == null) result += "null;";
            else
                for (int i = 0; i < Output.Length; ++i)
                    result += Output[i].ToString() + "; ";
            result += Environment.NewLine + "Error : ";

            if (error == null) result += "null;";
            else
                for (int i = 0; i < error.Length; ++i)
                    result += error[i].ToString() + "; ";
            result += Environment.NewLine + "Recognized : " + recognizedClass.ToString() + "(" +
                      (recognizedClass.ToInt()).ToString() + "); " + Environment.NewLine;


            return result;
        }

        /// <summary>
        /// Правильно ли распознан образ
        /// </summary>
        /// <returns></returns>
        public bool Correct()
        {
            return actualClass.Equals(recognizedClass);
        }
    }

    /// <summary>
    /// Выборка образов. Могут быть как классифицированные (обучающая, тестовая выборки), так и не классифицированные (обработка)
    /// </summary>
    public class SamplesSet<T> : IEnumerable where T: ISampleData, new()
    {
        /// <summary>
        /// Накопленные обучающие образы
        /// </summary>
        public List<Sample<T>> samples = new List<Sample<T>>();

        /// <summary>
        /// Добавление образа к коллекции
        /// </summary>
        /// <param name="image"></param>
        public void AddSample(Sample<T> image)
        {
            samples.Add(image);
        }

        public int Count => samples.Count;

        public IEnumerator GetEnumerator()
        {
            return samples.GetEnumerator();
        }

        /// <summary>
        /// Реализация доступа по индексу
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Sample<T> this[int i]
        {
            get => samples[i];
            set => samples[i] = value;
        }

        public double TestNeuralNetwork(BaseNetwork<T> network)
        {
            double correct = 0;
            double wrong = 0;
            foreach (var sample in samples)
            {
                if (sample.actualClass.Equals(network.Predict(sample))) ++correct;
                else ++wrong;
            }
            return correct / (correct + wrong);
        }

        // Тут бы ещё сохранение в файл и чтение сделать, вообще классно было бы
    }
}