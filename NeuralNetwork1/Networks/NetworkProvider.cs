using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork1
{
    /// <summary>
    /// класс-синглтон, содержащий нейросеть и её параметры
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class NetworkProvider<T> where T: ISampleData, new()
    {
        public static NetworkProvider<T> Get()
        {
            if (provider == null)
            {
                provider = new NetworkProvider<T>();
            }
            return provider;
        }

        private static NetworkProvider<T> provider;

        public BaseNetwork<T> Network
        {
            get
            {
                if (!networksCache.ContainsKey(SelectedNetwork))
                    networksCache.Add(SelectedNetwork, CreateNetwork(SelectedNetwork));

                return networksCache[SelectedNetwork];
            }
        }

        public int ImageSize => NetworkStructure[0]/2;

        

        public void Init(string selectedNetwork,
            Dictionary<string, Func<int[], BaseNetwork<T>>> networksFabric,
            int[] networkStructure,
            TrainProgressHandler updateLearningInfo)
        {
            SelectedNetwork = selectedNetwork;
            this.networksFabric = networksFabric;
            NetworkStructure = networkStructure;
            UpdateLearningInfo = updateLearningInfo;
        }

        public void Clean()
        {
            // Чистим старые подписки сетей
            foreach (var network in networksCache.Values)
                network.TrainProgress -= UpdateLearningInfo;
            // Пересоздаём все сети с новой структурой
            networksCache = networksCache.ToDictionary(oldNet => 
                oldNet.Key, oldNet => CreateNetwork(oldNet.Key));
        }
        
        private BaseNetwork<T> CreateNetwork(string networkName)
        {
            var network = networksFabric[networkName](NetworkStructure);
            network.TrainProgress += UpdateLearningInfo;
            return network;
        }

        public TrainProgressHandler UpdateLearningInfo { get; set; }

        public int[] NetworkStructure
        {
            get;
            set;
            //get
            //{
            //    return (int[])NetworkStructure.Clone();
            //}
            //set
            //{
            //    // if (value == null) return;
            //    //foreach (var i in value)
            //    //{
            //    //    if (i < 0) throw new ArgumentException("Количество нейронов в слое должно быть положительным");
            //    //}
            //    //if (value[0] % 2 != 0) throw new ArgumentException("Количество нейронов во входном слое должно быть четным");
            //    NetworkStructure = value; //int[])value.Clone();
            //}
        }

        public string SelectedNetwork { get; set; }
        private Dictionary<string, Func<int[], BaseNetwork<T>>> networksFabric;
        private Dictionary<string, BaseNetwork<T>> networksCache = new Dictionary<string, BaseNetwork<T>>();

    }
}
