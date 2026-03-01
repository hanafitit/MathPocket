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

        /// <summary>
        /// Вычисляет диапазон осей так, чтобы прямая выглядела наклонной,
        /// а обе ключевые точки (A и B) были хорошо видны.
        /// Диапазон адаптируется под крутизну наклона прямой.
        /// </summary>
        private static (double xMin, double xMax, double yMin, double yMax) CalcLinearRange(double k, double b)
        {
            // Две ключевые точки: A(0; b) и B(-b/k; 0)
            double xA = 0, yA = b;
            double xB = Math.Abs(k) > 1e-12 ? -b / k : 0;
            double yB = 0;

            // Центр между точками
            double cx = (xA + xB) / 2.0;
            double cy = (yA + yB) / 2.0;

            // Расстояние между ключевыми точками с отступом × 2.5
            double spanX = Math.Max(Math.Abs(xB - xA), 1.0) * 2.5;
            double spanY = Math.Max(Math.Abs(yA - yB), 1.0) * 2.5;

            // Соблюдаем пропорцию экрана (640×400 = 1.6),
            // увеличиваем меньший диапазон чтобы обе оси вмещали всё нужное
            double aspect = (double)Width / Height; // 1.6
            spanX = Math.Max(spanX, spanY * aspect);
            spanY = spanX / aspect;

            // Минимальный размер окна — не меньше 10 единиц по X
            spanX = Math.Max(spanX, 10.0);
            spanY = spanX / aspect;

            return (
                cx - spanX / 2.0, cx + spanX / 2.0,
                cy - spanY / 2.0, cy + spanY / 2.0
            );
        }

        public static byte[] LinearFunction(double k, double b)
        {
            var (xMin, xMax, yMin, yMax) = CalcLinearRange(k, b);
            var plt = new Plot();

            // ── Прямая (линия без маркеров) ───────────────────────
            double[] xs = Range(xMin, xMax);
            double[] ys = xs.Select(x => k * x + b).ToArray();
            var line = plt.Add.ScatterLine(xs, ys);
            line.Color     = Colors.RoyalBlue;
            line.LineWidth = 2.5f;

            // ── Жирные оси координат ──────────────────────────────
            var hLine = plt.Add.HorizontalLine(0);
            hLine.Color     = Colors.Black;
            hLine.LineWidth = 1.5f;
            var vLine = plt.Add.VerticalLine(0);
            vLine.Color     = Colors.Black;
            vLine.LineWidth = 1.5f;

            // ── Точка A: пересечение с осью Oy (x = 0) ───────────
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

            // ── Точка B: пересечение с осью Ox (y = 0) ───────────
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

            // ── Заголовок ─────────────────────────────────────────
            string title = k == 0  ? $"y = {FmtLabel(b)}"
                         : b == 0  ? $"y = {FmtLabel(k)}x"
                         : b > 0   ? $"y = {FmtLabel(k)}x + {FmtLabel(b)}"
                         :           $"y = {FmtLabel(k)}x − {FmtLabel(-b)}";
            plt.Title(title, size: 16);
            plt.XLabel("x");
            plt.YLabel("y");
            plt.ShowLegend(Alignment.LowerRight);

            plt.Axes.SetLimits(xMin, xMax, yMin, yMax);
            plt.Axes.SquareUnits();

            return plt.GetImageBytes(Width, Height, ImageFormat.Png);
        }

        /// <summary>
        /// График знака линейной функции: прямая + закрашенные области y>0 (зелёная) и y<0 (красная).
        /// </summary>
        public static byte[] LinearSignPlot(double k, double b)
        {
            var (xMin, xMax, yMin, yMax) = CalcLinearRange(k, b);
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
            var line = plt.Add.ScatterLine(xs, ys);
            line.Color     = Colors.RoyalBlue;
            line.LineWidth = 2.5f;

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

            // ── Подписи зон ───────────────────────────────────────
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

            string title = k == 0  ? $"y = {FmtLabel(b)}"
                         : b == 0  ? $"y = {FmtLabel(k)}x"
                         : b > 0   ? $"y = {FmtLabel(k)}x + {FmtLabel(b)}"
                         :           $"y = {FmtLabel(k)}x − {FmtLabel(-b)}";
            plt.Title(title, size: 16);
            plt.XLabel("x");
            plt.YLabel("y");
            plt.ShowLegend(Alignment.LowerRight);

            plt.Axes.SetLimits(xMin, xMax, yMin, yMax);
            plt.Axes.SquareUnits();

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
