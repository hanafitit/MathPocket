using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathPocket
{
    // ═══════════════════════════════════════════════════════════════
    //  Деление одночлена и многочлена на одночлен
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Деление одночлена на одночлен.
    /// Пример: 40xy³ ÷ 0,5y² = 80xy
    /// </summary>
    internal class MonomialDividePolyFunction : FunctionBase
    {
        public override string   Name       => "Одночлен ÷ одночлен";
        public override string   Formula    => "k₁xᵐ ÷ k₂xⁿ = (k₁/k₂)xᵐ⁻ⁿ";
        public override string[] Keywords   => new[] { "деление", "одночлен", "одночлен" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Деление одночлена на одночлен\n\n" +
                    "Правило: записываем дробь и сокращаем — " +
                    "коэффициенты делим, а степени переменных вычитаем.\n\n" +
                    "Пример: 40xy³ ÷ 0,5y²\n" +
                    "  = 40xy³ / 0,5y²\n" +
                    "  = (40/0,5) · x · y³⁻²\n" +
                    "  = 80xy\n\n" +
                    "Как записывать: x² → x^2, коэффициент перед x.\n\n" +
                    "✏️ Введи делимое (одночлен, например: 40x или 14x^3 или -6x^2):",
                Validate = PolyMultiplyHelper.ValidateMonomial
            },
            new InputStep
            {
                Question = "✏️ Введи делитель (одночлен, например: 0.5y^2 или -2x или 7):\n\n" +
                           "Примечание: используй одну переменную x для обоих.",
                Validate = PolyMultiplyHelper.ValidateMonomial
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var (k1, d1) = PolyMultiplyHelper.ParseMonomial(answers[0]);
            var (k2, d2) = PolyMultiplyHelper.ParseMonomial(answers[1]);

            if (k2 == 0) return "⚠️ Делитель не может быть нулём.";

            string mono1 = PolyMultiplyHelper.FormatMonomial(k1, d1);
            string mono2 = PolyMultiplyHelper.FormatMonomial(k2, d2);

            var sb = new StringBuilder();
            sb.AppendLine($"✅ ({mono1}) ÷ ({mono2})");
            sb.AppendLine();
            sb.AppendLine("Записываем как дробь и сокращаем:");
            sb.AppendLine();

            // Коэффициент
            bool kExact = k1 % k2 == 0;
            string kStr;
            if (kExact)
                kStr = (k1 / k2).ToString();
            else
            {
                double kD = (double)k1 / k2;
                kStr = kD == Math.Floor(kD) ? ((long)kD).ToString() : kD.ToString("G6");
            }
            sb.AppendLine($"  Коэффициенты: {k1} ÷ {k2} = {kStr}");

            // Степень
            int dR = d1 - d2;
            if (d1 > 0 || d2 > 0)
            {
                string xD1 = d1 == 0 ? "x⁰" : d1 == 1 ? "x" : "x" + PolyTerm.Sup(d1);
                string xD2 = d2 == 0 ? "x⁰" : d2 == 1 ? "x" : "x" + PolyTerm.Sup(d2);
                sb.AppendLine($"  Степени x:    {xD1} ÷ {xD2} = x{PolyTerm.Sup(dR)}  (показатели: {d1} − {d2} = {dR})");
            }

            if (dR < 0)
            {
                sb.AppendLine();
                sb.AppendLine("⚠️ Степень делителя больше степени делимого — результат будет дробным.");
                sb.AppendLine($"   Результат: {kStr}/x{PolyTerm.Sup(-dR)}");
            }
            else
            {
                string resultStr = PolyMultiplyHelper.FormatMonomial(k2 != 0 && kExact ? k1 / k2 : (long)Math.Round((double)k1/k2), dR);
                if (!kExact)
                {
                    double kD = (double)k1 / k2;
                    resultStr = (dR == 0 ? kD.ToString("G6") :
                                dR == 1 ? kD.ToString("G6") + "x" :
                                kD.ToString("G6") + "x" + PolyTerm.Sup(dR));
                }
                sb.AppendLine();
                sb.AppendLine($"📌 Ответ: {resultStr}");
            }

            return sb.ToString().TrimEnd();
        }
    }

    /// <summary>
    /// Деление многочлена на одночлен.
    /// Каждый член многочлена делим на одночлен.
    /// </summary>
    public class PolynomialDivideByMonomialFunction : FunctionBase
    {
        public override string   Name       => "Многочлен ÷ одночлен";
        public override string   Formula    => "(aₙxⁿ + … + a₀) ÷ kxᵐ";
        public override string[] Keywords   => new[] { "деление", "многочлен", "одночлен" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Деление многочлена на одночлен\n\n" +
                    "Правило: каждый член многочлена делим на одночлен.\n\n" +
                    "Пример: (−3,6a²b² + 3a²b + 44a⁴b) ÷ (−4a²b)\n" +
                    "  = −3,6a²b²/(−4a²b) + 3a²b/(−4a²b) + 44a⁴b/(−4a²b)\n" +
                    "  = 0,9b − 0,75 − 11a²\n\n" +
                    "✏️ Введи многочлен (делимое):",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question = "✏️ Введи одночлен-делитель (например: 3x^2 или -2x или 5):",
                Validate = PolyMultiplyHelper.ValidateMonomial
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var poly             = PolyParser.Parse(answers[0]);
            var (monoK, monoDeg) = PolyMultiplyHelper.ParseMonomial(answers[1]);
            string divisorStr    = PolyMultiplyHelper.FormatMonomial(monoK, monoDeg);

            if (monoK == 0) return "⚠️ Делитель не может быть нулём.";

            var sb = new StringBuilder();
            sb.AppendLine($"✅ ({PolyParser.Format(poly)}) ÷ ({divisorStr})");
            sb.AppendLine();
            sb.AppendLine("Делим каждый член многочлена на одночлен:");
            sb.AppendLine();

            var quotients     = new List<PolyTerm>();
            bool hasRemainder = false;

            foreach (var t in poly)
            {
                int  newDeg  = t.Degree - monoDeg;
                bool divides = newDeg >= 0 && t.Coeff % monoK == 0;

                if (!divides)
                {
                    // Показываем дробный результат
                    double kD    = (double)t.Coeff / monoK;
                    string kStr  = kD == Math.Floor(kD) ? ((long)kD).ToString() : kD.ToString("G6");
                    string resStr;
                    if (newDeg < 0)
                    {
                        resStr = kStr + ((-newDeg) == 1 ? "/x" : "/x" + PolyTerm.Sup(-newDeg));
                        hasRemainder = true;
                    }
                    else
                    {
                        resStr = newDeg == 0 ? kStr
                               : newDeg == 1 ? kStr + "x"
                               : kStr + "x" + PolyTerm.Sup(newDeg);
                    }
                    sb.AppendLine($"  ({t.ToStringFirst()}) ÷ ({divisorStr}) = {resStr}");
                    if (t.Coeff % monoK == 0 && newDeg >= 0)
                        quotients.Add(new PolyTerm(t.Coeff / monoK, newDeg));
                }
                else
                {
                    var q = new PolyTerm(t.Coeff / monoK, newDeg);
                    sb.AppendLine($"  ({t.ToStringFirst()}) ÷ ({divisorStr}) = {q.ToStringFirst()}");
                    quotients.Add(q);
                }
            }

            sb.AppendLine();

            if (hasRemainder)
            {
                sb.AppendLine("⚠️ Некоторые члены не делятся нацело — результат содержит дроби.");
                sb.AppendLine("   Проверь делитель или посмотри в раздел «Алгебраические дроби».");
            }
            else
            {
                var reduced = PolyParser.Reduce(quotients);

                // Проверка
                var check        = PolyMultiplyHelper.Multiply(reduced, new List<PolyTerm> { new PolyTerm(monoK, monoDeg) });
                var checkReduced = PolyParser.Reduce(check);
                var original     = PolyParser.Reduce(poly);

                sb.AppendLine($"📌 Результат: {PolyParser.Format(reduced)}");
                sb.AppendLine();

                if (PolyParser.Format(checkReduced) == PolyParser.Format(original))
                    sb.AppendLine($"✓ Проверка: ({PolyParser.Format(reduced)}) · ({divisorStr}) = {PolyParser.Format(original)} ✓");
            }

            return sb.ToString().TrimEnd();
        }
    }

    /// <summary>
    /// Упростить выражение с делением (многочлен / одночлен ± многочлен / одночлен).
    /// </summary>
    public class PolyDivideSimplifyFunction : FunctionBase
    {
        public override string   Name       => "Упростить выражение с делением";
        public override string   Formula    => "A÷k ± B÷m → упростить каждое слагаемое";
        public override string[] Keywords   => new[] { "упростить", "деление", "выражение" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Упрощение выражения с делением\n\n" +
                    "Пример: 40x·y : (8x − 6xy) + 1,4(5 − xy) : (3x + 2,5)\n\n" +
                    "Шаги:\n" +
                    "  1. Делим каждый многочлен на свой одночлен\n" +
                    "  2. Складываем результаты\n" +
                    "  3. Приводим подобные\n\n" +
                    "✏️ Введи первый многочлен (числитель 1-го дроби):",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question = "✏️ Введи делитель 1-й дроби (одночлен):",
                Validate = PolyMultiplyHelper.ValidateMonomial
            },
            new InputStep
            {
                Question =
                    "Есть вторая дробь?\n\n" +
                    "Напиши знак, числитель и делитель через / (пример: +6x^2-3x / 3x)\n" +
                    "Или напиши 0 если нет.",
                Validate = s =>
                {
                    s = s.Trim();
                    if (s == "0") return null;
                    var parts = s.Split('/');
                    if (parts.Length < 2) return "Напиши числитель / делитель (или 0)";
                    try
                    {
                        PolyParser.Parse(parts[0].TrimStart('+').Trim());
                        PolyMultiplyHelper.ValidateMonomial(parts[parts.Length - 1].Trim());
                        return null;
                    }
                    catch (FormatException ex) { return ex.Message; }
                }
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var poly1             = PolyParser.Parse(answers[0]);
            var (k1, d1)          = PolyMultiplyHelper.ParseMonomial(answers[1]);
            string divisor1       = PolyMultiplyHelper.FormatMonomial(k1, d1);

            var sb = new StringBuilder();
            sb.AppendLine("✅ Упрощаем выражение:");
            sb.AppendLine();

            var allTerms = new List<PolyTerm>();

            // 1-я дробь
            sb.AppendLine($"Часть 1: ({PolyParser.Format(poly1)}) ÷ ({divisor1})");
            var q1 = new List<PolyTerm>();
            foreach (var t in poly1)
            {
                int  nd = t.Degree - d1;
                bool ok = nd >= 0 && t.Coeff % k1 == 0;
                if (ok)
                {
                    var qt = new PolyTerm(t.Coeff / k1, nd);
                    sb.AppendLine($"  {t.ToStringFirst()} ÷ {divisor1} = {qt.ToStringFirst()}");
                    q1.Add(qt);
                }
                else
                {
                    double kD = (double)t.Coeff / k1;
                    string rs = kD.ToString("G6") + (nd < 0 ? "/x" + (nd < -1 ? PolyTerm.Sup(-nd) : "") : nd == 0 ? "" : nd == 1 ? "x" : "x" + PolyTerm.Sup(nd));
                    sb.AppendLine($"  {t.ToStringFirst()} ÷ {divisor1} = {rs}  ⚠️ не делится нацело");
                }
            }
            var r1 = PolyParser.Reduce(q1);
            sb.AppendLine($"  = {PolyParser.Format(r1)}");
            allTerms.AddRange(r1);

            // 2-я дробь
            if (answers[2].Trim() != "0")
            {
                var slash  = answers[2].LastIndexOf('/');
                string num = answers[2].Substring(0, slash).TrimStart('+').Trim();
                string den = answers[2].Substring(slash + 1).Trim();

                int sign       = num.TrimStart().StartsWith("-") ? -1 : 1;
                var poly2      = PolyParser.Parse(num.TrimStart('-').TrimStart('+').Trim());
                var (k2, d2)   = PolyMultiplyHelper.ParseMonomial(den);
                string div2    = PolyMultiplyHelper.FormatMonomial(k2, d2);

                sb.AppendLine();
                sb.AppendLine($"Часть 2: ({num.TrimStart('+').Trim()}) ÷ ({div2})");
                var q2 = new List<PolyTerm>();
                foreach (var t in poly2)
                {
                    int  nd = t.Degree - d2;
                    bool ok = nd >= 0 && t.Coeff % k2 == 0;
                    if (ok)
                    {
                        var qt = new PolyTerm(sign * t.Coeff / k2, nd);
                        sb.AppendLine($"  {t.ToStringFirst()} ÷ {div2} = {qt.ToStringFirst()}");
                        q2.Add(qt);
                    }
                    else
                    {
                        sb.AppendLine($"  {t.ToStringFirst()} ÷ {div2}  ⚠️ не делится нацело");
                    }
                }
                var r2 = PolyParser.Reduce(q2);
                sb.AppendLine($"  = {PolyParser.Format(r2)}");
                allTerms.AddRange(r2);
            }

            var total   = PolyParser.Reduce(allTerms);
            var groups  = allTerms.GroupBy(t => t.Degree).Where(g => g.Count() > 1).ToList();

            if (groups.Any())
            {
                sb.AppendLine();
                sb.AppendLine("Приводим подобные:");
                foreach (var g in groups.OrderByDescending(g => g.Key))
                {
                    string lbl   = g.Key == 0 ? "свободные члены"
                                 : g.Key == 1 ? "члены с x"
                                 : $"члены с x{PolyTerm.Sup(g.Key)}";
                    string chain = string.Join(" + ", g.Select(t => t.Coeff.ToString()))
                                        .Replace("+ -", "− ");
                    sb.AppendLine($"  {lbl}: {chain} = {g.Sum(t => t.Coeff)}");
                }
            }

            sb.AppendLine();
            sb.AppendLine($"📌 Результат: {PolyParser.Format(total)}");
            return sb.ToString().TrimEnd();
        }
    }

    /// <summary>
    /// Найти значение выражения с делением при заданных значениях переменной.
    /// </summary>
    public class PolyDivideEvalFunction : FunctionBase
    {
        public override string   Name       => "Значение выражения с делением";
        public override string   Formula    => "(A ÷ k) при x = n";
        public override string[] Keywords   => new[] { "значение", "деление", "подставить", "при x" };
        public override string[] Parameters => Array.Empty<string>();
        public override double   Calculate(double[] _) => throw new NotSupportedException();

        public override InputStep[] Steps => new[]
        {
            new InputStep
            {
                Question =
                    "📘 Значение выражения с делением\n\n" +
                    "Сначала делим многочлен на одночлен (упрощаем), " +
                    "потом подставляем значение.\n\n" +
                    "Пример: 100b⁴ · 4b² − 5b при b = 0,2\n" +
                    "  Шаг 1. Делим: 100b⁴ ÷ 4b² = 25b²\n" +
                    "  Шаг 2. Подставляем: 25·(0,2)² − 5·0,2 = 25·0,04 − 1 = 0\n\n" +
                    "✏️ Введи многочлен (числитель):",
                Validate = PolyValidate.CheckPoly
            },
            new InputStep
            {
                Question = "✏️ Введи делитель (одночлен):",
                Validate = PolyMultiplyHelper.ValidateMonomial
            },
            new InputStep
            {
                Question = "✏️ Введи значение x:",
                Validate = PolyValidate.CheckNumber
            }
        };

        public override string CalculateFromAnswers(List<string> answers)
        {
            var poly             = PolyParser.Parse(answers[0]);
            var (monoK, monoDeg) = PolyMultiplyHelper.ParseMonomial(answers[1]);
            string divisorStr    = PolyMultiplyHelper.FormatMonomial(monoK, monoDeg);

            double xVal = double.Parse(answers[2].Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture);

            var sb = new StringBuilder();
            sb.AppendLine($"✅ ({PolyParser.Format(poly)}) ÷ ({divisorStr}) при x = {answers[2]}");
            sb.AppendLine();
            sb.AppendLine("Шаг 1. Делим каждый член:");

            var quotients = new List<PolyTerm>();
            bool allOk    = true;
            foreach (var t in poly)
            {
                int  nd = t.Degree - monoDeg;
                bool ok = nd >= 0 && t.Coeff % monoK == 0;
                if (ok)
                {
                    var q = new PolyTerm(t.Coeff / monoK, nd);
                    sb.AppendLine($"  {t.ToStringFirst()} ÷ {divisorStr} = {q.ToStringFirst()}");
                    quotients.Add(q);
                }
                else
                {
                    double kD   = (double)t.Coeff / monoK;
                    string kStr = kD.ToString("G6");
                    sb.AppendLine($"  {t.ToStringFirst()} ÷ {divisorStr} = {kStr}{(nd > 0 ? "x" + (nd > 1 ? PolyTerm.Sup(nd) : "") : "")}  (нецелое)");
                    allOk = false;
                }
            }

            var reduced = PolyParser.Reduce(quotients);
            sb.AppendLine();
            sb.AppendLine($"  Упрощённый многочлен: {PolyParser.Format(reduced)}");
            sb.AppendLine();
            sb.AppendLine($"Шаг 2. Подставляем x = {answers[2]}:");

            double result = 0;
            foreach (var t in reduced)
            {
                double term = t.Coeff * Math.Pow(xVal, t.Degree);
                result += term;
                string xStr = t.Degree == 0 ? "" : t.Degree == 1 ? $"·{xVal}" : $"·{xVal}^{t.Degree}";
                sb.AppendLine($"  {t.Coeff}{xStr} = {term:G10}".TrimEnd('0').TrimEnd('.'));
            }

            string resultStr = result == Math.Floor(result) ? ((long)result).ToString() : result.ToString("G10").TrimEnd('0').TrimEnd('.');
            sb.AppendLine();
            sb.AppendLine($"📌 Ответ: {resultStr}");
            return sb.ToString().TrimEnd();
        }
    }
}
