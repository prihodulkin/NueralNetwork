using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork1.SampleGeneration
{
    public enum AlphabetLetter : byte {Alpha = 0, Beta, Chi, Delta, Epsilon, Eta, Gamma, Iota, Kappa, Lambda, Mu, Undef};
    internal class AlphabetSampleData : ISampleData
    {
        public AlphabetLetter Letter { get; private set; }
        public AlphabetSampleData() { }

        public AlphabetSampleData(AlphabetLetter letter)
        {
            Letter = letter;
        }

        public void ByInt(int i)
        {
            Letter = (AlphabetLetter)i;
        }

        public bool Equals(ISampleData other)
        {
            if(other is AlphabetSampleData)
            {
                return Letter == (other as AlphabetSampleData).Letter;
            }
            return false;
        }

        public bool IsUndefined()
        {
            return Letter == AlphabetLetter.Undef;
        }

        public int ToInt()
        {
           return (int) Letter;
        }

        public override string ToString()
        {
            return Letter.ToString();
        }
    }
}
