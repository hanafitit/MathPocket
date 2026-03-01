using System;
using System.Collections.Generic;
using System.Linq;
using ScottPlot;
using ScottPlot.TickGenerators;

namespace MathPocket
{
    /// <summary>
    /// Вспомогательный класс для построения графиков через ScottPlot 5.
    /// Все методы возвращают байты PNG — ничего не сохраняется на диск.
    /// </summary>
    internal static class PlotHelper
    {
        public const int Width  = 700;
        public const int Height = 500;

        // ─── Scatter / линейный график ────────────────────────────

        public static byte[] Line(
            double[] xs, double[] ys,
            string title  = "",
            string xLabel = "x",
            string yLabel = "y")
        {
            var plt = new Plot();

            var scatter = plt.Add.Scatter(xs, ys);
            scatter.Color      = Colors.RoyalBlue;
            scatter.LineWidth  = 2;
            scatter.MarkerSize = 0;

            AddAxes(plt);
            plt.Title(title);
            plt.XLabel(xLabel);
            plt.YLabel(yLabel);

            return plt.GetImageBytes(Width, Height, ImageFormat.Png);
        }

        // ─── Вычисление диапазона ─────────────────────────────────

        /// <summary>
        /// Возвращает диапазон осей и шаг тика.
        /// Окно: 10 клеток по X, 6 по Y — каждая клетка = tickStep единиц.
        /// </summary>
        private static (double xMin, double xMax, double yMin, double yMax, double tickStep)
            CalcLinearRange(double k, double b)
        {
            // Ключевые точки: A(0; b) и B(-b/k; 0)
            double xA = 0, yA = b;
            double xB = Math.Abs(k) > 1e-12 ? -b / k : 0;

            double cx = (xA + xB) / 2.0;
            double cy = (yA + 0)  / 2.0;

            // Шаг всегда = 1: одна клетка = одна единица
            double step = 1.0;

            // Привязка центра к сетке
            double cxSnap = Math.Round(cx / step) * step;
            double cySnap = Math.Round(cy / step) * step;

            // Базовое окно 10×8 клеток
            double xMin = cxSnap - 5.0;
            double xMax = cxSnap + 5.0;
            double yMin = cySnap - 4.0;
            double yMax = cySnap + 4.0;

            // Гарантируем что обе ключевые точки видны с отступом 1.5 клетки
            double pad = 1.5;
            if (xA - pad < xMin) { double shift = xMin - (xA - pad); xMin -= shift; xMax -= shift; }
            if (xA + pad > xMax) { double shift = (xA + pad) - xMax; xMin += shift; xMax += shift; }
            if (xB - pad < xMin) { double shift = xMin - (xB - pad); xMin -= shift; xMax -= shift; }
            if (xB + pad > xMax) { double shift = (xB + pad) - xMax; xMin += shift; xMax += shift; }
            if (yA - pad < yMin) { double shift = yMin - (yA - pad); yMin -= shift; yMax -= shift; }
            if (yA + pad > yMax) { double shift = (yA + pad) - yMax; yMin += shift; yMax += shift; }

            // Гарантируем что начало координат видно с отступом 1 клетка
            if (0 - pad < xMin) { double shift = xMin - (0 - pad); xMin -= shift; xMax -= shift; }
            if (0 + pad > xMax) { double shift = (0 + pad) - xMax; xMin += shift; xMax += shift; }
            if (0 - pad < yMin) { double shift = yMin - (0 - pad); yMin -= shift; yMax -= shift; }
            if (0 + pad > yMax) { double shift = (0 + pad) - yMax; yMin += shift; yMax += shift; }

            return (xMin, xMax, yMin, yMax, step);
        }

        private static double NiceNumber(double x)
        {
            if (x <= 0) return 1;
            double mag  = Math.Pow(10, Math.Floor(Math.Log10(x)));
            double norm = x / mag;
            double nice = norm < 1.5 ? 1 : norm < 3.5 ? 2 : norm < 7.5 ? 5 : 10;
            return nice * mag;
        }

        // ─── Применение тиков (ScottPlot 5) ──────────────────────

        private static void ApplyTicks(Plot plt,
            double xMin, double xMax, double yMin, double yMax, double step)
        {
            double[] xPos = MakePositions(xMin, xMax, step);
            double[] yPos = MakePositions(yMin, yMax, step);

            string[] xLbl = xPos.Select(v => FormatTick(v, step)).ToArray();
            string[] yLbl = yPos.Select(v => FormatTick(v, step)).ToArray();

            // ScottPlot 5: задаём тики через NumericManual
            var xGen = new NumericManual(xPos, xLbl);
            var yGen = new NumericManual(yPos, yLbl);

            plt.Axes.Bottom.TickGenerator = xGen;
            plt.Axes.Left.TickGenerator   = yGen;

            plt.Axes.Bottom.TickLabelStyle.FontSize = 11;
            plt.Axes.Left.TickLabelStyle.FontSize   = 11;
        }

        private static double[] MakePositions(double min, double max, double step)
        {
            var list  = new List<double>();
            double v0 = Math.Ceiling(min / step) * step;
            for (double v = v0; v <= max + step * 1e-6; v += step)
                list.Add(Math.Round(v / step) * step);
            return list.ToArray();
        }

        private static string FormatTick(double v, double step)
        {
            long iv = (long)Math.Round(v);
            return Math.Abs(v - iv) < step * 1e-6 ? iv.ToString() : v.ToString("G4");
        }

        // ─── LinearFunction ───────────────────────────────────────

        public static byte[] LinearFunction(double k, double b)
        {
            var (xMin, xMax, yMin, yMax, step) = CalcLinearRange(k, b);
            var plt = new Plot();
            plt.Layout.Fixed(new ScottPlot.PixelPadding(50, 20, 40, 20));

            // Прямая
            double[] xs = Range(xMin, xMax);
            double[] ys = xs.Select(x => k * x + b).ToArray();
            var line = plt.Add.ScatterLine(xs, ys);
            line.Color     = Colors.RoyalBlue;
            line.LineWidth = 2.5f;

            // Оси
            var hLine = plt.Add.HorizontalLine(0);
            hLine.Color     = Colors.Black;
            hLine.LineWidth = 1.5f;
            var vLine = plt.Add.VerticalLine(0);
            vLine.Color     = Colors.Black;
            vLine.LineWidth = 1.5f;

            // Точка A: пересечение с Oy
            var oyMarker = plt.Add.Marker(0, b);
            oyMarker.Color      = Colors.OrangeRed;
            oyMarker.Size       = 12;
            oyMarker.LegendText = $"A(0; {FmtLabel(b)})";

            var oyText = plt.Add.Text($"A(0; {FmtLabel(b)})", 0, b);
            oyText.LabelFontSize        = 13;
            oyText.LabelFontColor       = Colors.OrangeRed;
            oyText.LabelBold            = true;
            oyText.LabelAlignment       = Alignment.LowerLeft;
            oyText.LabelBorderWidth     = 0;
            oyText.LabelBackgroundColor = Colors.Transparent;

            // Точка B: пересечение с Ox
            if (Math.Abs(k) > 1e-12)
            {
                double x0 = -b / k;
                if (x0 >= xMin && x0 <= xMax)
                {
                    var oxMarker = plt.Add.Marker(x0, 0);
                    oxMarker.Color      = Colors.SeaGreen;
                    oxMarker.Size       = 12;
                    oxMarker.LegendText = $"B({FmtLabel(x0)}; 0)";

                    var oxText = plt.Add.Text($"B({FmtLabel(x0)}; 0)", x0, 0);
                    oxText.LabelFontSize        = 13;
                    oxText.LabelFontColor       = Colors.SeaGreen;
                    oxText.LabelBold            = true;
                    oxText.LabelAlignment       = Alignment.UpperRight;
                    oxText.LabelBorderWidth     = 0;
                    oxText.LabelBackgroundColor = Colors.Transparent;
                }
            }

            // Заголовок
            string title = k == 0 ? $"y = {FmtLabel(b)}"
                         : b == 0 ? $"y = {FmtLabel(k)}x"
                         : b > 0  ? $"y = {FmtLabel(k)}x + {FmtLabel(b)}"
                         :          $"y = {FmtLabel(k)}x − {FmtLabel(-b)}";
            plt.Title(title, size: 16);
            plt.XLabel("x");
            plt.YLabel("y");
            plt.ShowLegend(Alignment.LowerRight);

            plt.Axes.SetLimits(xMin, xMax, yMin, yMax);
            ApplyTicks(plt, xMin, xMax, yMin, yMax, step);

            return plt.GetImageBytes(Width, Height, ImageFormat.Png);
        }

        // ─── LinearSignPlot ───────────────────────────────────────

        public static byte[] LinearSignPlot(double k, double b)
        {
            var (xMin, xMax, yMin, yMax, step) = CalcLinearRange(k, b);
            var plt = new Plot();
            plt.Layout.Fixed(new ScottPlot.PixelPadding(50, 20, 40, 20));

            double[] xs    = Range(xMin, xMax);
            double[] ys    = xs.Select(x => k * x + b).ToArray();
            double[] zeros = xs.Select(_ => 0.0).ToArray();
            double[] ysPos = ys.Select(y => Math.Max(y, 0)).ToArray();
            double[] ysNeg = ys.Select(y => Math.Min(y, 0)).ToArray();

            // Закрашенные области
            var fillPos = plt.Add.FillY(xs, zeros, ysPos);
            fillPos.FillColor = Colors.LightGreen.WithAlpha(0.45f);
            fillPos.LineWidth = 0;

            var fillNeg = plt.Add.FillY(xs, ysNeg, zeros);
            fillNeg.FillColor = Colors.LightCoral.WithAlpha(0.45f);
            fillNeg.LineWidth = 0;

            // Прямая
            var line = plt.Add.ScatterLine(xs, ys);
            line.Color     = Colors.RoyalBlue;
            line.LineWidth = 2.5f;

            // Оси
            var hLine = plt.Add.HorizontalLine(0);
            hLine.Color     = Colors.Black;
            hLine.LineWidth = 1.5f;
            var vLine = plt.Add.VerticalLine(0);
            vLine.Color     = Colors.Black;
            vLine.LineWidth = 1.5f;

            // Нуль функции
            if (Math.Abs(k) > 1e-12)
            {
                double x0 = -b / k;
                if (x0 >= xMin && x0 <= xMax)
                {
                    var zeroMarker = plt.Add.Marker(x0, 0);
                    zeroMarker.Color      = Colors.RoyalBlue;
                    zeroMarker.Size       = 12;
                    zeroMarker.LegendText = $"x₀ = {FmtLabel(x0)}";

                    var zeroText = plt.Add.Text($"x₀={FmtLabel(x0)}", x0, 0);
                    zeroText.LabelFontSize        = 13;
                    zeroText.LabelFontColor       = Colors.RoyalBlue;
                    zeroText.LabelBold            = true;
                    zeroText.LabelAlignment       = Alignment.UpperRight;
                    zeroText.LabelBorderWidth     = 0;
                    zeroText.LabelBackgroundColor = Colors.Transparent;
                }
            }

            // Подписи зон
            var posLabel = plt.Add.Annotation("y > 0");
            posLabel.Alignment            = Alignment.UpperRight;
            posLabel.LabelFontColor       = Colors.DarkGreen;
            posLabel.LabelFontSize        = 14;
            posLabel.LabelBorderWidth     = 0;
            posLabel.LabelBackgroundColor = Colors.Transparent;

            var negLabel = plt.Add.Annotation("y < 0");
            negLabel.Alignment            = Alignment.LowerRight;
            negLabel.LabelFontColor       = Colors.DarkRed;
            negLabel.LabelFontSize        = 14;
            negLabel.LabelBorderWidth     = 0;
            negLabel.LabelBackgroundColor = Colors.Transparent;

            string title = k == 0 ? $"y = {FmtLabel(b)}"
                         : b == 0 ? $"y = {FmtLabel(k)}x"
                         : b > 0  ? $"y = {FmtLabel(k)}x + {FmtLabel(b)}"
                         :          $"y = {FmtLabel(k)}x − {FmtLabel(-b)}";
            plt.Title(title, size: 16);
            plt.XLabel("x");
            plt.YLabel("y");
            plt.ShowLegend(Alignment.LowerRight);

            plt.Axes.SetLimits(xMin, xMax, yMin, yMax);
            ApplyTicks(plt, xMin, xMax, yMin, yMax, step);

            return plt.GetImageBytes(Width, Height, ImageFormat.Png);
        }

        // ─── Вспомогательное ─────────────────────────────────────

        private static string FmtLabel(double v)
        {
            if (v == Math.Floor(v) && Math.Abs(v) < 1e12) return ((long)v).ToString();
            return v.ToString("G4", System.Globalization.CultureInfo.InvariantCulture);
        }

        public static byte[] Bar(
            double[] values,
            string[] labels,
            string title = "")
        {
            var plt = new Plot();
            plt.Add.Bars(values);

            if (labels.Length == values.Length)
            {
                var positions = Enumerable.Range(0, labels.Length)
                    .Select(i => (double)i).ToArray();
                plt.Axes.Bottom.SetTicks(positions, labels);
            }

            plt.Title(title);
            return plt.GetImageBytes(Width, Height, ImageFormat.Png);
        }

        private static void AddAxes(Plot plt)
        {
            plt.Add.HorizontalLine(0, color: Colors.Black, width: 1);
            plt.Add.VerticalLine(0,   color: Colors.Black, width: 1);
        }

        public static double[] Range(double xMin, double xMax, int points = 400)
        {
            var xs   = new double[points];
            double s = (xMax - xMin) / (points - 1);
            for (int i = 0; i < points; i++)
                xs[i] = xMin + i * s;
            return xs;
        }
    }
}
