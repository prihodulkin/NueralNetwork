using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork1
{
    /// <summary>
    /// Интерфейс обёртки над enum объектов, которые надо классифицировать.
    /// Необходимо переопределить ToString(), возвращая ToString() enum-a
    /// </summary>
    public interface ISampleData: IEquatable<ISampleData>
    {
        /// <summary>
        /// является ли элемент Undef - такой класс должен быть в каждом энаме, true после вызова конструктора без параметров
        /// </summary>
        /// <returns></returns>
        bool IsUndefined();

        /// <summary>
        /// перевод enum-a в int
        /// </summary>
        /// <returns></returns>
        int ToInt();

        /// <summary>
        /// установка значения i для enum-a
        /// </summary>
        /// <param name="i"></param>
        void ByInt(int i);
        
    }
}
