using System;
using System.Collections.Generic;
using System.Linq;
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
        /// График линейной функции y = kx + b.
        /// Рисует прямую, оси, нуль функции и пересечение с Oy.
        /// </summary>
        public static byte[] LinearFunction(
            double k, double b,
            double xMin = -10, double xMax = 10)
        {
            var plt = new Plot();

            // ── Прямая ────────────────────────────────────────────
            double[] xs = Range(xMin, xMax);
            double[] ys = xs.Select(x => k * x + b).ToArray();

            var line = plt.Add.Scatter(xs, ys);
            line.Color      = Colors.RoyalBlue;
            line.LineWidth  = 2.5f;
            line.MarkerSize = 0;

            // ── Жирные оси координат ──────────────────────────────
            var hLine = plt.Add.HorizontalLine(0);
            hLine.Color     = Colors.Black;
            hLine.LineWidth = 1.5f;

            var vLine = plt.Add.VerticalLine(0);
            vLine.Color     = Colors.Black;
            vLine.LineWidth = 1.5f;

            // ── Точка пересечения с осью Oy (x = 0) ──────────────
            var oyPoint = plt.Add.Scatter(new[] { 0.0 }, new[] { b });
            oyPoint.Color      = Colors.OrangeRed;
            oyPoint.MarkerSize = 12;
            oyPoint.LineWidth  = 0;
            oyPoint.LegendText = $"A(0; {FmtLabel(b)})  — ось Oy";

            // ── Подпись точки Oy ──────────────────────────────────
            var oyLabel = plt.Add.Text($"A(0; {FmtLabel(b)})", 0, b);
            oyLabel.LabelFontSize = 13;
            oyLabel.LabelFontColor = Colors.OrangeRed;
            oyLabel.LabelAlignment = ScottPlot.Alignment.LowerLeft;
            oyLabel.LabelBorderWidth = 0;
            oyLabel.LabelBackgroundColor = Colors.Transparent;

            // ── Точка пересечения с осью Ox (y = 0) ──────────────
            if (Math.Abs(k) > 1e-12)
            {
                double x0 = -b / k;
                if (x0 >= xMin && x0 <= xMax)
                {
                    var oxPoint = plt.Add.Scatter(new[] { x0 }, new[] { 0.0 });
                    oxPoint.Color      = Colors.SeaGreen;
                    oxPoint.MarkerSize = 12;
                    oxPoint.LineWidth  = 0;
                    oxPoint.LegendText = $"B({FmtLabel(x0)}; 0)  — ось Ox";

                    var oxLabel = plt.Add.Text($"B({FmtLabel(x0)}; 0)", x0, 0);
                    oxLabel.LabelFontSize = 13;
                    oxLabel.LabelFontColor = Colors.SeaGreen;
                    oxLabel.LabelAlignment = ScottPlot.Alignment.UpperLeft;
                    oxLabel.LabelBorderWidth = 0;
                    oxLabel.LabelBackgroundColor = Colors.Transparent;
                }
            }

            // ── Заголовок и подписи осей ──────────────────────────
            string title = k == 0  ? $"y = {FmtLabel(b)}"
                         : b == 0  ? $"y = {FmtLabel(k)}x"
                         : b > 0   ? $"y = {FmtLabel(k)}x + {FmtLabel(b)}"
                         :           $"y = {FmtLabel(k)}x − {FmtLabel(-b)}";

            plt.Title(title, size: 16);
            plt.XLabel("x");
            plt.YLabel("y");
            plt.ShowLegend(ScottPlot.Alignment.LowerRight);

            return plt.GetImageBytes(Width, Height, ImageFormat.Png);
        }

        /// <summary>
        /// График знака линейной функции: прямая + закрашенные области y>0 (зелёная) и y<0 (красная).
        /// </summary>
        public static byte[] LinearSignPlot(double k, double b, double xMin = -10, double xMax = 10)
        {
            var plt = new Plot();

            double[] xs    = Range(xMin, xMax);
            double[] ys    = xs.Select(x => k * x + b).ToArray();
            double[] zeros = xs.Select(_ => 0.0).ToArray();
            double[] ysPos = ys.Select(y => Math.Max(y, 0)).ToArray();
            double[] ysNeg = ys.Select(y => Math.Min(y, 0)).ToArray();

            // ── Закрашенные области ───────────────────────────────
            var fillPos = plt.Add.FillY(xs, zeros, ysPos);
            fillPos.FillColor = Colors.LightGreen.WithAlpha(0.45f);
            fillPos.LineWidth = 0;

            var fillNeg = plt.Add.FillY(xs, ysNeg, zeros);
            fillNeg.FillColor = Colors.LightCoral.WithAlpha(0.45f);
            fillNeg.LineWidth = 0;

            // ── Прямая ────────────────────────────────────────────
            var line = plt.Add.Scatter(xs, ys);
            line.Color      = Colors.RoyalBlue;
            line.LineWidth  = 2.5f;
            line.MarkerSize = 0;

            // ── Жирные оси ────────────────────────────────────────
            var hLine = plt.Add.HorizontalLine(0);
            hLine.Color     = Colors.Black;
            hLine.LineWidth = 1.5f;

            var vLine = plt.Add.VerticalLine(0);
            vLine.Color     = Colors.Black;
            vLine.LineWidth = 1.5f;

            // ── Нуль функции ──────────────────────────────────────
            if (Math.Abs(k) > 1e-12)
            {
                double x0 = -b / k;
                if (x0 >= xMin && x0 <= xMax)
                {
                    var zeroMark = plt.Add.Scatter(new[] { x0 }, new[] { 0.0 });
                    zeroMark.Color      = Colors.RoyalBlue;
                    zeroMark.MarkerSize = 12;
                    zeroMark.LineWidth  = 0;
                    zeroMark.LegendText = $"x₀ = {FmtLabel(x0)}  (нуль функции)";
                }
            }

            // ── Подписи зон ───────────────────────────────────────
            var posLabel = plt.Add.Annotation("y > 0");
            posLabel.Alignment            = ScottPlot.Alignment.UpperRight;
            posLabel.LabelFontColor       = Colors.DarkGreen;
            posLabel.LabelFontSize        = 14;
            posLabel.LabelBorderWidth     = 0;
            posLabel.LabelBackgroundColor = Colors.Transparent;

            var negLabel = plt.Add.Annotation("y < 0");
            negLabel.Alignment            = ScottPlot.Alignment.LowerRight;
            negLabel.LabelFontColor       = Colors.DarkRed;
            negLabel.LabelFontSize        = 14;
            negLabel.LabelBorderWidth     = 0;
            negLabel.LabelBackgroundColor = Colors.Transparent;

            string title = k == 0  ? $"y = {FmtLabel(b)}"
                         : b == 0  ? $"y = {FmtLabel(k)}x"
                         : b > 0   ? $"y = {FmtLabel(k)}x + {FmtLabel(b)}"
                         :           $"y = {FmtLabel(k)}x − {FmtLabel(-b)}";

            plt.Title(title, size: 16);
            plt.XLabel("x");
            plt.YLabel("y");
            plt.ShowLegend(ScottPlot.Alignment.LowerRight);

            return plt.GetImageBytes(Width, Height, ImageFormat.Png);
        }

        private static string FmtLabel(double v)
        {
            if (v == Math.Floor(v) && Math.Abs(v) < 1e12) return ((long)v).ToString();
            return v.ToString("G4", System.Globalization.CultureInfo.InvariantCulture);
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

            // Подписи по оси X
            if (labels.Length == values.Length)
            {
                var positions = Enumerable.Range(0, labels.Length)
                    .Select(i => (double)i)
                    .ToArray();
                plt.Axes.Bottom.SetTicks(positions, labels);
            }

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
