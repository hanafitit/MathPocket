using System;
using System.Collections.Generic;
using System.Linq;

namespace MathPocket
{
    public abstract partial class FunctionBase
    {
        public virtual string Name { get; } = string.Empty;
        public virtual string Formula => string.Empty;
        public virtual string[] Parameters => [];
        public virtual string[] Keywords => [];

        public abstract double Calculate(double[] inputs);

        public virtual string CalculateFromText(string input)
        {
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != Parameters.Length)
                return $"Ожидалось {Parameters.Length} данных, получено {parts.Length}.";

            double[] numbers;
            try
            {
                numbers = parts
                    .Select(p => double.Parse(p.Replace(',', '.'),
                        System.Globalization.CultureInfo.InvariantCulture))
                    .ToArray();
            }
            catch (FormatException)
            {
                return "Ошибка: введите числа через пробел (например: 3 4.5).";
            }

            return Calculate(numbers).ToString();
        }

        public virtual InputStep[]? Steps => null;

        public virtual string CalculateFromAnswers(List<string> answers) =>
            throw new NotImplementedException($"{GetType().Name} не реализует CalculateFromAnswers.");

        public virtual int ActiveStepCount(List<string> answers) =>
            Steps?.Length ?? 0;

        public virtual string? GetPreview(List<string> answers) => null;

        public virtual byte[]? GetPlotBytes(List<string> answers) => null;
    }
}
