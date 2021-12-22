﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.WindowsForms;

namespace NeuralNetwork1
{
    public partial class NeuralNetworksStand<T> : Form where T: ISampleData, new()
    {
        /// <summary>
        /// Генератор изображений (образов)
        /// </summary>
        IGenerator<T> generator;


        /// <summary>
        /// Конструктор формы стенда для работы с сетями
        /// </summary>
        /// <param name="networksFabric">Словарь функций, создающих сети с заданной структурой</param>
        public NeuralNetworksStand(Dictionary<string, Func<int[], BaseNetwork<T>>> networksFabric, IGenerator<T> generator)
        {
            InitializeComponent();
            this.generator = generator;
            netTypeBox.Items.AddRange(networksFabric.Keys.Select(s => (object) s).ToArray());
            netTypeBox.SelectedIndex = 0;
             NetworkProvider<T>.Get().Init((string) netTypeBox.SelectedItem, 
                networksFabric,
                CurrentNetworkStructure(),
                UpdateLearningInfo);

            generator.ClassesCount = (int) classCounter.Value;
            button3_Click(this, null);
            pictureBox1.Image = Properties.Resources.Title;
        }

        public void UpdateLearningInfo(double progress, double error, TimeSpan elapsedTime)
        {
            if (progressBar1.InvokeRequired)
            {
                progressBar1.Invoke(new TrainProgressHandler(UpdateLearningInfo), progress, error, elapsedTime);
                return;
            }

            StatusLabel.Text = "Ошибка: " + error;
            int progressPercent = (int) Math.Round(progress * 100);
            progressPercent = Math.Min(100, Math.Max(0, progressPercent));
            elapsedTimeLabel.Text = "Затраченное время : " + elapsedTime.Duration().ToString(@"hh\:mm\:ss\:ff");
            progressBar1.Value = progressPercent;
        }


        private void set_result(Sample<T> figure)
        {
            label1.ForeColor = figure.Correct() ? Color.Green : Color.Red;

            label1.Text = "Распознано : " + figure.recognizedClass;

            label8.Text = string.Join("\n", figure.Output.Select(d => d.ToString(CultureInfo.InvariantCulture)));
            pictureBox1.Image = generator.GenerateBitmap();
            pictureBox1.Invalidate();
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            Sample<T> fig = generator.GenerateFigure();

             NetworkProvider<T>.Get().Network.Predict(fig);

            set_result(fig);
        }

        private async Task<double> train_networkAsync(int training_size, int epoches, double acceptable_error,
            bool parallel = true)
        {
            //  Выключаем всё ненужное
            label1.Text = "Выполняется обучение...";
            label1.ForeColor = Color.Red;
            groupBox1.Enabled = false;
            pictureBox1.Enabled = false;
            trainOneButton.Enabled = false;

            //  Создаём новую обучающую выборку
            SamplesSet<T> samples = new SamplesSet<T>();

            for (int i = 0; i < training_size; i++)
                samples.AddSample(generator.GenerateFigure());
            try
            {
                //  Обучение запускаем асинхронно, чтобы не блокировать форму
                var curNet =  NetworkProvider<T>.Get().Network;
                double f = await Task.Run(() => curNet.TrainOnDataSet(samples, epoches, acceptable_error, parallel));
                label1.Text = "Щелкните на картинку для теста нового образа";
                label1.ForeColor = Color.Green;
                groupBox1.Enabled = true;
                pictureBox1.Enabled = true;
                trainOneButton.Enabled = true;
                StatusLabel.Text = "Ошибка: " + f;
                StatusLabel.ForeColor = Color.Green;
                if (curNet is EvgenNetwork<T>)
                {
                    (curNet as EvgenNetwork<T>).Save("network.net");
                }
                return f;
            }
            catch (Exception e)
            {
                label1.Text = $"Исключение: {e.Message}";
            }

            return 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            train_networkAsync((int) TrainingSizeCounter.Value, (int) EpochesCounter.Value,
                (100 - AccuracyCounter.Value) / 100.0, parallelCheckBox.Checked);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Enabled = false;
            //  Тут просто тестирование новой выборки
            //  Создаём новую обучающую выборку
            SamplesSet<T> samples = new SamplesSet<T>();

            for (int i = 0; i < (int) TrainingSizeCounter.Value; i++)
                samples.AddSample(generator.GenerateFigure());

            double accuracy = samples.TestNeuralNetwork( NetworkProvider<T>.Get().Network);

            StatusLabel.Text = $"Точность на тестовой выборке : {accuracy * 100,5:F2}%";
            StatusLabel.ForeColor = accuracy * 100 >= AccuracyCounter.Value ? Color.Green : Color.Red;

            Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //  Проверяем корректность задания структуры сети
            int[] structure = CurrentNetworkStructure();
            if (structure.Length < 2 || structure[0] != 400 ||
                structure[structure.Length - 1] != generator.ClassesCount)
            {
                MessageBox.Show(
                    $"В сети должно быть более двух слоёв, первый слой должен содержать 400 нейронов, последний - ${generator.ClassesCount}",
                    "Ошибка", MessageBoxButtons.OK);
                return;
            }

            // Чистим старые подписки сетей
             NetworkProvider<T>.Get().Clean();
             NetworkProvider<T>.Get().NetworkStructure = structure;
        }

        private int[] CurrentNetworkStructure()
        {
            return netStructureBox.Text.Split(';').Select(int.Parse).ToArray();
        }

        private void classCounter_ValueChanged(object sender, EventArgs e)
        {
            generator.ClassesCount = (int) classCounter.Value;
            var vals = netStructureBox.Text.Split(';');
            if (!int.TryParse(vals.Last(), out _)) return;
            vals[vals.Length - 1] = classCounter.Value.ToString();
            netStructureBox.Text = vals.Aggregate((partialPhrase, word) => $"{partialPhrase};{word}");
        }

        private void btnTrainOne_Click(object sender, EventArgs e)
        {
            if ( NetworkProvider<T>.Get().Network == null) return;
            Sample<T> fig = generator.GenerateFigure();
            pictureBox1.Image = generator.GenerateBitmap();
            pictureBox1.Invalidate();
             NetworkProvider<T>.Get().Network.Train(fig, 0.00005, parallelCheckBox.Checked);
            set_result(fig);
        }

        private void recreateNetButton_MouseEnter(object sender, EventArgs e)
        {
            infoStatusLabel.Text = "Заново пересоздаёт сеть с указанными параметрами";
        }

        private void netTrainButton_MouseEnter(object sender, EventArgs e)
        {
            infoStatusLabel.Text = "Обучить нейросеть с указанными параметрами";
        }

        private void testNetButton_MouseEnter(object sender, EventArgs e)
        {
            infoStatusLabel.Text = "Тестировать нейросеть на тестовой выборке такого же размера";
        }

        private void testNetWithCameraButton_Click(object sender, EventArgs e)
        {
            new CameraForm<T>().Show();
        }

        private void netTypeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
             NetworkProvider<T>.Get().SelectedNetwork = (string) netTypeBox.SelectedItem;
        }
    }
}