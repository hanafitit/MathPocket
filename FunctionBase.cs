using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
namespace MathPocket
{
    public abstract partial class FunctionBase
    {
        public virtual string Name { get; }
        public virtual string Formula => "";
        public virtual string[] Parameters { get; }
        public virtual string[] Keywords => Array.Empty<string>();
        public abstract double Calculate(double[] inputs);
        public virtual string CalculateFromText(string input)
        {
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != Parameters.Length)
                return $"Ожидалось {Parameters.Length} данных";
            var numbers = parts.Select(p => double.Parse(p.Replace(',', '.'))).ToArray();
            return Calculate(numbers).ToString();
        }

        /// <summary>
        /// Превью текущего состояния после очередного ответа пользователя.
        /// Показывается перед следующим вопросом — чтобы пользователь видел что уже ввёл.
        /// Вернуть null если превью не нужно на данном шаге.
        /// </summary>
        public virtual string? GetPreview(List<string> answers) => null;
    }
}
