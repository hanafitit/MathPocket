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

        // ─── Two Linear Functions ─────────────────────────────────

        public static byte[] TwoLinearFunctions(double k1, double b1, double k2, double b2)
        {
            // Диапазон: стандартный 10×10
            double xMin = -10.0, xMax = 10.0, yMin = -10.0, yMax = 10.0, step = 1.0;

            // Расширяем если точки не влезают
            double maxCoord = Math.Max(
                Math.Max(Math.Abs(b1), Math.Abs(b2)),
                Math.Max(
                    Math.Abs(k1) > 1e-12 ? Math.Abs(b1 / k1) : 0,
                    Math.Abs(k2) > 1e-12 ? Math.Abs(b2 / k2) : 0));

            if (maxCoord > 8.0)
            {
                step = NiceNumber(maxCoord / 7.0);
                step = Math.Max(step, 1.0);
                xMin = -10.0 * step; xMax = 10.0 * step;
                yMin = -10.0 * step; yMax = 10.0 * step;
            }

            var plt = new Plot();

            // Прямая 1
            double[] xs1 = Range(xMin, xMax);
            double[] ys1 = xs1.Select(x => k1 * x + b1).ToArray();
            var line1 = plt.Add.ScatterLine(xs1, ys1);
            line1.Color       = Colors.RoyalBlue;
            line1.LineWidth   = 2.5f;
            line1.LegendText  = FormatFunc(k1, b1);

            // Прямая 2
            double[] xs2 = Range(xMin, xMax);
            double[] ys2 = xs2.Select(x => k2 * x + b2).ToArray();
            var line2 = plt.Add.ScatterLine(xs2, ys2);
            line2.Color       = Colors.OrangeRed;
            line2.LineWidth   = 2.5f;
            line2.LegendText  = FormatFunc(k2, b2);

            // Оси
            var hLine = plt.Add.HorizontalLine(0);
            hLine.Color = Colors.Black; hLine.LineWidth = 1.5f;
            var vLine = plt.Add.VerticalLine(0);
            vLine.Color = Colors.Black; vLine.LineWidth = 1.5f;

            // Точка пересечения
            if (Math.Abs(k1 - k2) > 1e-9)
            {
                double xi = (b2 - b1) / (k1 - k2);
                double yi = k1 * xi + b1;
                if (xi >= xMin && xi <= xMax && yi >= yMin && yi <= yMax)
                {
                    var dot = plt.Add.Marker(xi, yi);
                    dot.Color = Colors.SeaGreen;
                    dot.Size  = 12;
                    dot.LegendText = $"({FmtLabel(xi)}; {FmtLabel(yi)})";
                }
            }

            plt.ShowLegend(Alignment.LowerRight);
            ApplyTicks(plt, xMin, xMax, yMin, yMax, step);
            plt.Axes.SetLimits(xMin, xMax, yMin, yMax);

            return plt.GetImageBytes(Width, Height, ImageFormat.Png);
        }

        private static string FormatFunc(double k, double b)
        {
            string kStr = k == 1 ? "" : k == -1 ? "-" : FmtLabel(k);
            if (k == 0) return $"y = {FmtLabel(b)}";
            if (b == 0) return $"y = {kStr}x";
            if (b > 0)  return $"y = {kStr}x + {FmtLabel(b)}";
            return             $"y = {kStr}x − {FmtLabel(-b)}";
        }

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

        // ─── Диапазон для параболы y = ax² ───────────────────────

        private static (double xMin, double xMax, double yMin, double yMax, double step)
            CalcQuadraticRange(double a, double extraY = 0)
        {
            double halfX  = 5.0;
            double yAtEdge = Math.Abs(a) * halfX * halfX;
            double ySpan   = Math.Max(Math.Max(yAtEdge, Math.Abs(extraY)) * 1.3, 5.0);

            double step = NiceNumber(ySpan / 5.0);
            step  = Math.Max(step, 0.5);
            halfX = Math.Ceiling(halfX / step) * step;
            ySpan = Math.Ceiling(ySpan / step) * step;

            double yMin = a > 0 ? -step   : -ySpan;
            double yMax = a > 0 ?  ySpan  :  step;
            return (-halfX, halfX, yMin, yMax, step);
        }

        // ─── QuadraticPlot: y = ax² ───────────────────────────────

        public static byte[] QuadraticPlot(double a)
        {
            var (xMin, xMax, yMin, yMax, step) = CalcQuadraticRange(a);
            var plt = new Plot();

            double[] xs = Range(xMin, xMax);
            var curve = plt.Add.ScatterLine(xs, xs.Select(x => a * x * x).ToArray());
            curve.Color = Colors.RoyalBlue; curve.LineWidth = 2.5f;
            curve.LegendText = a == 1 ? "y = x²" : a == -1 ? "y = −x²" : $"y = {FmtLabel(a)}x²";

            plt.Add.HorizontalLine(0).Color = Colors.Black;
            plt.Add.VerticalLine(0).Color   = Colors.Black;

            var vt = plt.Add.Text("O(0;0)", 0, 0);
            vt.LabelFontSize = 12; vt.LabelFontColor = Colors.OrangeRed;
            vt.LabelAlignment = Alignment.UpperRight;
            vt.LabelBorderWidth = 0; vt.LabelBackgroundColor = Colors.Transparent;

            plt.Title(curve.LegendText, size: 16);
            plt.XLabel("x"); plt.YLabel("y");
            plt.ShowLegend(Alignment.LowerRight);
            ApplyTicks(plt, xMin, xMax, yMin, yMax, step);
            plt.Axes.SetLimits(xMin, xMax, yMin, yMax);
            return plt.GetImageBytes(Width, Height, ImageFormat.Png);
        }

        // ─── TwoQuadraticPlot: y = a₁x² и y = a₂x² ─────────────

        public static byte[] TwoQuadraticPlot(double a1, double a2)
        {
            double aBig = Math.Abs(a1) > Math.Abs(a2) ? a1 : a2;
            var (xMin, xMax, yMin, yMax, step) = CalcQuadraticRange(aBig);
            var plt = new Plot();

            double[] xs = Range(xMin, xMax);

            var c1 = plt.Add.ScatterLine(xs, xs.Select(x => a1 * x * x).ToArray());
            c1.Color = Colors.RoyalBlue; c1.LineWidth = 2.5f;
            c1.LegendText = a1 == 1 ? "y = x²" : a1 == -1 ? "y = −x²" : $"y = {FmtLabel(a1)}x²";

            var c2 = plt.Add.ScatterLine(xs, xs.Select(x => a2 * x * x).ToArray());
            c2.Color = Colors.OrangeRed; c2.LineWidth = 2.5f;
            c2.LegendText = a2 == 1 ? "y = x²" : a2 == -1 ? "y = −x²" : $"y = {FmtLabel(a2)}x²";

            plt.Add.HorizontalLine(0).Color = Colors.Black;
            plt.Add.VerticalLine(0).Color   = Colors.Black;

            plt.Title($"{c1.LegendText}  и  {c2.LegendText}", size: 14);
            plt.XLabel("x"); plt.YLabel("y");
            plt.ShowLegend(Alignment.LowerRight);
            ApplyTicks(plt, xMin, xMax, yMin, yMax, step);
            plt.Axes.SetLimits(xMin, xMax, yMin, yMax);
            return plt.GetImageBytes(Width, Height, ImageFormat.Png);
        }

        // ─── QuadraticWithLine: парабола + горизонтальная y = c ──

        public static byte[] QuadraticWithLine(double a, double c)
        {
            var (xMin, xMax, yMin, yMax, step) = CalcQuadraticRange(a, c);
            var plt = new Plot();

            double[] xs = Range(xMin, xMax);
            var curve = plt.Add.ScatterLine(xs, xs.Select(x => a * x * x).ToArray());
            curve.Color = Colors.RoyalBlue; curve.LineWidth = 2.5f;
            curve.LegendText = $"y = {FmtLabel(a)}x²";

            var hline = plt.Add.HorizontalLine(c);
            hline.Color = Colors.OrangeRed; hline.LineWidth = 2f;
            hline.LegendText = $"y = {FmtLabel(c)}";

            plt.Add.HorizontalLine(0).Color = Colors.Black;
            plt.Add.VerticalLine(0).Color   = Colors.Black;

            // Точки пересечения
            double ratio = c / a;
            if (ratio > 1e-9)
            {
                double xr = Math.Sqrt(ratio);
                foreach (double xi in new[] { -xr, xr })
                {
                    if (xi >= xMin && xi <= xMax)
                    {
                        var m = plt.Add.Marker(xi, c);
                        m.Color = Colors.SeaGreen; m.Size = 10;
                        m.LegendText = $"({FmtLabel(xi)}; {FmtLabel(c)})";
                    }
                }
            }
            else if (Math.Abs(ratio) < 1e-9)
            {
                var m = plt.Add.Marker(0, 0);
                m.Color = Colors.SeaGreen; m.Size = 10;
            }

            plt.ShowLegend(Alignment.LowerRight);
            ApplyTicks(plt, xMin, xMax, yMin, yMax, step);
            plt.Axes.SetLimits(xMin, xMax, yMin, yMax);
            return plt.GetImageBytes(Width, Height, ImageFormat.Png);
        }

        // ─── QuadraticWithLinearLine: парабола + прямая kx+b ─────

        public static byte[] QuadraticWithLinearLine(double a, double k, double b)
        {
            double maxY = Math.Max(Math.Abs(b), Math.Abs(a * 5 * 5));
            var (xMin, xMax, yMin, yMax, step) = CalcQuadraticRange(a, maxY);
            var plt = new Plot();

            double[] xs = Range(xMin, xMax);

            var curve = plt.Add.ScatterLine(xs, xs.Select(x => a * x * x).ToArray());
            curve.Color = Colors.RoyalBlue; curve.LineWidth = 2.5f;
            curve.LegendText = a == 1 ? "y = x²" : a == -1 ? "y = −x²" : $"y = {FmtLabel(a)}x²";

            var line = plt.Add.ScatterLine(xs, xs.Select(x => k * x + b).ToArray());
            line.Color = Colors.OrangeRed; line.LineWidth = 2f;
            line.LegendText = FormatFunc(k, b);

            plt.Add.HorizontalLine(0).Color = Colors.Black;
            plt.Add.VerticalLine(0).Color   = Colors.Black;

            // Точки пересечения: ax² - kx - b = 0
            double A = a, B = -k, C = -b;
            double D = B * B - 4 * A * C;
            if (D >= 0)
            {
                double sqrtD = Math.Sqrt(D);
                foreach (double xi in new[] { (-B - sqrtD) / (2 * A), (-B + sqrtD) / (2 * A) })
                {
                    double yi = a * xi * xi;
                    if (xi >= xMin && xi <= xMax && yi >= yMin && yi <= yMax)
                    {
                        var m = plt.Add.Marker(xi, yi);
                        m.Color = Colors.SeaGreen; m.Size = 10;
                        m.LegendText = $"({FmtLabel(xi)}; {FmtLabel(yi)})";
                    }
                }
            }

            plt.ShowLegend(Alignment.LowerRight);
            ApplyTicks(plt, xMin, xMax, yMin, yMax, step);
            plt.Axes.SetLimits(xMin, xMax, yMin, yMax);
            return plt.GetImageBytes(Width, Height, ImageFormat.Png);
        }

        // ─── QuadraticOnInterval: парабола с выделенным участком ─

        public static byte[] QuadraticOnInterval(double a, double x1, double x2)
        {
            if (x1 > x2) (x1, x2) = (x2, x1);
            double maxY = Math.Max(Math.Abs(a * x1 * x1), Math.Abs(a * x2 * x2));
            var (xMin, xMax, yMin, yMax, step) = CalcQuadraticRange(a, maxY);
            var plt = new Plot();

            double[] xs = Range(xMin, xMax);

            // Фоновая парабола
            var bg = plt.Add.ScatterLine(xs, xs.Select(x => a * x * x).ToArray());
            bg.Color = Colors.RoyalBlue.WithAlpha(0.2f); bg.LineWidth = 1.5f;

            // Выделенный участок
            double[] xSeg = Range(x1, x2, 200);
            var seg = plt.Add.ScatterLine(xSeg, xSeg.Select(x => a * x * x).ToArray());
            seg.Color = Colors.RoyalBlue; seg.LineWidth = 3.5f;
            seg.LegendText = $"y={FmtLabel(a)}x² на [{FmtLabel(x1)};{FmtLabel(x2)}]";

            // Концы
            foreach (double xi in new[] { x1, x2 })
            {
                double yi = a * xi * xi;
                var m = plt.Add.Marker(xi, yi);
                m.Color = Colors.OrangeRed; m.Size = 10;
                var t = plt.Add.Text($"({FmtLabel(xi)};{FmtLabel(yi)})", xi, yi);
                t.LabelFontSize = 11; t.LabelFontColor = Colors.OrangeRed;
                t.LabelAlignment = Alignment.UpperRight;
                t.LabelBorderWidth = 0; t.LabelBackgroundColor = Colors.Transparent;
            }

            plt.Add.HorizontalLine(0).Color = Colors.Black;
            plt.Add.VerticalLine(0).Color   = Colors.Black;

            plt.ShowLegend(Alignment.LowerRight);
            ApplyTicks(plt, xMin, xMax, yMin, yMax, step);
            plt.Axes.SetLimits(xMin, xMax, yMin, yMax);
            return plt.GetImageBytes(Width, Height, ImageFormat.Png);
        }
    }
}
