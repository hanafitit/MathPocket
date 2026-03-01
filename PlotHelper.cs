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
            double xB = Math.Abs(k) > 1e-12 ? -b / k : 0;

            // Определяем нужный масштаб по максимальному из координат ключевых точек
            double maxCoord = Math.Max(Math.Abs(b), Math.Abs(xB));
            maxCoord = Math.Max(maxCoord, 1.0); // минимум 1 чтобы не схлопнуться

            // Выбираем шаг тика чтобы окно 5 клеток вмещало все точки с отступом
            double rawStep = maxCoord / 3.5; // 3.5 клетки от центра до точки
            double step = NiceNumber(rawStep);
            step = Math.Max(step, 1.0); // шаг не меньше 1

            // Окно всегда центрировано вокруг (0, 0): 5 клеток в каждую сторону по X, 4 по Y
            double xMin = -5.0 * step;
            double xMax =  5.0 * step;
            double yMin = -4.0 * step;
            double yMax =  4.0 * step;

            // Гарантируем что ключевые точки видны с отступом 1.5 клетки
            double pad = 1.5 * step;
            if (xB - pad < xMin) xMin = xB - pad;
            if (xB + pad > xMax) xMax = xB + pad;
            if (b  - pad < yMin) yMin = b  - pad;
            if (b  + pad > yMax) yMax = b  + pad;

            // Гарантируем что начало координат всегда видно с отступом 1 клетка
            double originPad = step;
            if (-originPad < xMin) xMin = -originPad;
            if ( originPad > xMax) xMax =  originPad;
            if (-originPad < yMin) yMin = -originPad;
            if ( originPad > yMax) yMax =  originPad;

            // Округляем границы до кратных step
            xMin = Math.Floor(xMin / step) * step;
            xMax = Math.Ceiling(xMax / step) * step;
            yMin = Math.Floor(yMin / step) * step;
            yMax = Math.Ceiling(yMax / step) * step;

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

            // SetTicks() — встроенный helper, заменяет TickGenerator на NumericManual
            plt.Axes.Bottom.SetTicks(xPos, xLbl);
            plt.Axes.Left.SetTicks(yPos, yLbl);

            plt.Axes.Bottom.TickLabelStyle.FontSize = 12;
            plt.Axes.Left.TickLabelStyle.FontSize   = 12;
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

            ApplyTicks(plt, xMin, xMax, yMin, yMax, step);
            plt.Axes.SetLimits(xMin, xMax, yMin, yMax);

            return plt.GetImageBytes(Width, Height, ImageFormat.Png);
        }

        // ─── LinearSignPlot ───────────────────────────────────────

        public static byte[] LinearSignPlot(double k, double b)
        {
            var (xMin, xMax, yMin, yMax, step) = CalcLinearRange(k, b);
            var plt = new Plot();

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

            ApplyTicks(plt, xMin, xMax, yMin, yMax, step);
            plt.Axes.SetLimits(xMin, xMax, yMin, yMax);

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
