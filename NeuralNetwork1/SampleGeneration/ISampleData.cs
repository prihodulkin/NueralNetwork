using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork1
{
    public interface ISampleData: IEquatable<ISampleData>
    {
        bool IsUndefined();
         int ToInt();
        void ByInt(int i);
        
    }
}
