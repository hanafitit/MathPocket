using System;
using System.Collections.Generic;
using ScottPlot;

namespace MathPocket
{
    /// <summary>
    /// Вспомогательный класс для построения графиков через ScottPlot.
    /// Все методы возвращают байты PNG — ничего не сохраняется на диск.
    /// </summary>
    internal static class PlotHelper
    {
        public const int Width  = 640;
        public const int Height = 400;

        // ─── Scatter / линейный график ────────────────────────────

        /// <summary>
        /// Простой линейный график по точкам xs/ys.
        /// </summary>
        public static byte[] Line(
            double[] xs, double[] ys,
            string title  = "",
            string xLabel = "x",
            string yLabel = "y")
        {
            var plt = new Plot();

            var scatter = plt.Add.Scatter(xs, ys);
            scatter.Color     = Colors.RoyalBlue;
            scatter.LineWidth = 2;
            scatter.MarkerSize = 0;

            AddAxes(plt);

            plt.Title(title);
            plt.XLabel(xLabel);
            plt.YLabel(yLabel);

            return plt.GetImageBytes(Width, Height, ImageFormat.Png);
        }

        /// <summary>
        /// Столбчатая диаграмма.
        /// </summary>
        public static byte[] Bar(
            double[] values,
            string[] labels,
            string title = "")
        {
            var plt = new Plot();

            var bar = plt.Add.Bars(values);

            plt.Title(title);

            return plt.GetImageBytes(Width, Height, ImageFormat.Png);
        }

        // ─── Вспомогательное ─────────────────────────────────────

        private static void AddAxes(Plot plt)
        {
            plt.Add.HorizontalLine(0, color: Colors.Black, width: 1);
            plt.Add.VerticalLine(0,   color: Colors.Black, width: 1);
        }

        /// <summary>
        /// Сгенерировать равномерно распределённые x от xMin до xMax.
        /// </summary>
        public static double[] Range(double xMin, double xMax, int points = 400)
        {
            var xs = new double[points];
            double step = (xMax - xMin) / (points - 1);
            for (int i = 0; i < points; i++)
                xs[i] = xMin + i * step;
            return xs;
        }
    }
}
