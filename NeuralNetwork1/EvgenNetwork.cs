using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork1
{
    class EvgenNetwork:BaseNetwork
    {
        private double [][,] Weights;
        private double[][] Values;
        private double[][] Errors;
        public double Speed = 0.25;
        public Stopwatch stopWatch = new Stopwatch();

        public EvgenNetwork(int [] structure, double lowRandomBound=-1, double upRandomBound=1)
        {
            Values = new double[structure.Length][];
            Errors = new double[structure.Length][];
            for (int i = 0; i < structure.Length; i++)
            {
                Errors[i] = new double[structure[i]];
                Values[i] = new double[structure[i]+1];
                Values[i][structure[i]] = 1;
            }
            Weights = new double[structure.Length - 1][,];
            for(int n = 0; n < structure.Length-1; n++)
            {
                var rowsCount = structure[n]+1;
                var columnsCount = structure[n+1];
                Weights[n] = new double[rowsCount, columnsCount];
                Random random = new Random();
                for(int i = 0; i < rowsCount; i++)
                {
                    for(int j=0; j<columnsCount; j++)
                    {
                        Weights[n][i, j] = random.NextDouble() * (upRandomBound - lowRandomBound) + lowRandomBound;
                    }
                }
            }
        }

        public double Error(double[] output)
        {
            double result = 0;
            for(int i = 0; i < output.Length; i++)
            {
                result += (Math.Pow(output[i] - Values[Values.Length - 1][i], 2));
            }
            result /= output.Length;
            return result;
        }

        public void ComputeValues(double[] input)
        {
            for(int j = 0; j < input.Length; j++)
            {
                Values[0][j] = input[j];
            }
            for(int i=1; i < Values.GetLength(0); i++)
            {
                Values[i - 1].MutliplyAndApplySigmoid(Weights[i-1], Values[i]);
            }
        }

        public void ComputeValuesParallel(double[] input)
        {
            Parallel.For(0, input.Length, (j) =>
            {
                Values[0][j] = input[j];
            });
            
            for (int i = 1; i < Values.GetLength(0); i++)
            {
                Values[i - 1].MutliplyAndApplySigmoidParallel(Weights[i - 1], Values[i]);
            }
        }

        public void ComputeErrors(double[] output)
        {
            for (var j = 0; j < output.Length; j++)
            {
                var actualValue = Values[Errors.Length - 1][j];
                var expectedValue = output[j];
                Errors[Errors.Length - 1][j] = actualValue * (1 - actualValue) * (expectedValue - actualValue);
            }
            for (int i = Errors.Length - 2; i >= 1; i--)
            {
                for (int j = 0; j < Errors[i].Length; j++)
                {
                    var value = Values[i][j];
                    value = value * (1 - value);
                    var sum = 0.0;
                    for (int k = 0; k < Errors[i + 1].Length; k++)
                    {
                        sum += Errors[i + 1][k] * Weights[i][j, k];
                    }
                    Errors[i][j] = value * sum;
                }
            }
        }

        public void ComputeErrorsParallel(double[] output)
        {
            Parallel.For(0, output.Length, (j) =>
            {
                var actualValue = Values[Errors.Length - 1][j];
                var expectedValue = output[j];
                Errors[Errors.Length - 1][j] = actualValue * (1 - actualValue) * (expectedValue - actualValue);
            });
            for (int i = Errors.Length - 2; i >= 1; i--)
            {
                for (int j = 0; j < Errors[i].Length; j++)
                {
                    var value = Values[i][j];
                    value = value * (1 - value);
                    var sum = 0.0;
                    Parallel.For(0, Errors[i + 1].Length, k =>
                    {
                        sum += Errors[i + 1][k] * Weights[i][j, k];
                    });
                    Errors[i][j] = value * sum;
                }
            }
        }

        public void ComputeWeights()
        {
           
            for(int n = 0; n < Weights.Length; n++)
            {
                for(int i = 0; i < Weights[n].GetLength(0); i++)
                {
                    for(int j = 0; j < Weights[n].GetLength(1); j++)
                    {
                        var dWeight = Speed * Errors[n + 1][j] * Values[n][i];
                        Weights[n][i, j] += dWeight;
                    }
                }
            }
        }

        public void ComputeWeightsParallel()
        {
            for (int n = 0; n < Weights.Length; n++)
            {
                Parallel.For(0, Weights[n].GetLength(0), i =>
                {
                    Parallel.For(0, Weights[n].GetLength(1), j =>
                    {
                        var dWeight = Speed * Errors[n + 1][j] * Values[n][i];
                        Weights[n][i, j] += dWeight;
                    });
                });
            }
        }

        private int SimpleTrain(Sample sample, double acceptableError)
        {
            int iters = 1;
            while (Error(sample.Output) > acceptableError)
            {
                ComputeValuesParallel(sample.input);
                ComputeErrorsParallel(sample.Output);
                ComputeWeightsParallel();
                ++iters;
            }

            return iters;
        }

        private int ParallelTrain(Sample sample, double acceptableError)
        {
            int iters = 1;
            while (Error(sample.Output) > acceptableError)
            {
                ComputeValues(sample.input);
                ComputeErrors(sample.Output);
                ComputeWeights();
                ++iters;
            }

            return iters;
        }

        public override int Train(Sample sample, double acceptableError, bool parallel)
        {
            return parallel ?ParallelTrain(sample, acceptableError):SimpleTrain(sample, acceptableError);
        }


        public override double TrainOnDataSet(SamplesSet samplesSet, int epochsCount, double acceptableError, bool parallel)
        {
            //  Сначала надо сконструировать массивы входов и выходов
            double[][] inputs = new double[samplesSet.Count][];
            double[][] outputs = new double[samplesSet.Count][];

            //  Теперь массивы из samplesSet группируем в inputs и outputs
            for (int i = 0; i < samplesSet.Count; ++i)
            {
                inputs[i] = samplesSet[i].input;
                outputs[i] = samplesSet[i].Output;
            }

            //  Текущий счётчик эпох
            int epoch_to_run = 0;

            double error = double.PositiveInfinity;

#if DEBUG
            StreamWriter errorsFile = File.CreateText("errors.csv");
#endif

            stopWatch.Restart();

            while (epoch_to_run < epochsCount && error > acceptableError)
            {
                epoch_to_run++;
                error = 0;
                for (int i = 0; i < inputs.Length; i++)
                {
                    if (parallel)
                    {
                        ComputeValuesParallel(inputs[i]);
                        ComputeErrorsParallel(outputs[i]);
                        ComputeWeightsParallel();
                    }
                    else
                    {
                        ComputeValues(inputs[i]);
                        ComputeErrors(outputs[i]);
                        ComputeWeights();
                    }
                    error += Error(outputs[i]);
                }
                error /= inputs.Length;
                //errorsFile.WriteLine(error);
                OnTrainProgress((epoch_to_run * 1.0) / epochsCount, error, stopWatch.Elapsed);
            }

#if DEBUG
            errorsFile.Close();
#endif
            OnTrainProgress((epoch_to_run * 1.0) / epochsCount, error, stopWatch.Elapsed);
            stopWatch.Stop();
            return error;
        }

        protected override double[] Compute(double[] input)
        {
            ComputeValues(input);
            return Values.Last().Take(Values.Last().Length - 1).ToArray();
        }
    }
}
