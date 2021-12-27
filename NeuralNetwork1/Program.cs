using NeuralNetwork1.SampleGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeuralNetwork1
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new NeuralNetworksStand<FigureSampleData> (new Dictionary<string, Func<int[], BaseNetwork<FigureSampleData>>>
            //{
            //    // Тут можно добавить свои нейросети
            //    {"Accord.Network Perseptron", structure => new AccordNet<FigureSampleData>(structure)},
            //    {"Студентческий персептрон", structure => new EvgenNetwork<FigureSampleData>("network.net")},
            //}, new FiguresGenerator()));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new NeuralNetworksStand<AlphabetSampleData > (new Dictionary<string, Func<int[], BaseNetwork<AlphabetSampleData>>>
            {
                // Тут можно добавить свои нейросети
                {"Accord.Network Perseptron", structure => new AccordNet<AlphabetSampleData>(structure)},
                {"Студентческий персептрон", structure => new EvgenNetwork<AlphabetSampleData>(structure)},
            }, new AlphabetGenerator()));
        }
    }
}