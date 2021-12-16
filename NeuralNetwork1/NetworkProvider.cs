using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork1
{
    class NetworkProvider
    {
        public static NetworkProvider Get()
        {
            if (provider == null)
            {
                provider = new NetworkProvider();
            }
            return provider;
        }

        private static NetworkProvider provider;

        public BaseNetwork Network
        {
            get
            {
                if (!networksCache.ContainsKey(SelectedNetwork))
                    networksCache.Add(SelectedNetwork, CreateNetwork(SelectedNetwork));

                return networksCache[SelectedNetwork];
            }
        }

        public void Init(string selectedNetwork,
            Dictionary<string, Func<int[], BaseNetwork>> networksFabric,
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
        
        private BaseNetwork CreateNetwork(string networkName)
        {
            var network = networksFabric[networkName](NetworkStructure);
            network.TrainProgress += UpdateLearningInfo;
            return network;
        }

        public TrainProgressHandler UpdateLearningInfo { get; set; }
        public int[] NetworkStructure { get; set; }
        public string SelectedNetwork { get; set; }
        private Dictionary<string, Func<int[], BaseNetwork>> networksFabric;
        private Dictionary<string, BaseNetwork> networksCache = new Dictionary<string, BaseNetwork>();

    }
}
