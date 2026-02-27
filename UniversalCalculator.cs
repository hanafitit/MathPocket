using System;
using System.Collections.Generic;
using System.Linq;
namespace MathPocket
{
    public class Fraction
    {
        public int Numerator { get; private set; }
        public int Denominator { get; private set; }
        public Fraction(string text)
        {
            if (text.Contains("/"))
            {
                var parts = text.Split('/');
                Numerator = int.Parse(parts[0]);
                Denominator = int.Parse(parts[1]);
            }
            else if (text.Contains("."))
            {
                // Десятичное число: 36.7 → 367/10
                int dotPos = text.IndexOf('.');
                int decimals = text.Length - dotPos - 1;
                Denominator = (int)Math.Pow(10, decimals);
                Numerator = int.Parse(text.Replace(".", ""));
            }
            else
            {
                Numerator = int.Parse(text);
                Denominator = 1;
            }
            Reduce();
        }
        public Fraction(int num, int den)
        {
            if (den == 0) throw new DivideByZeroException();
            if (den < 0) { num = -num; den = -den; }
            int gcd = GCD(Math.Abs(num), den);
            Numerator = num / gcd;
            Denominator = den / gcd;
        }
        private void Reduce()
        {
            if (Denominator < 0) { Numerator = -Numerator; Denominator = -Denominator; }
            int gcd = GCD(Math.Abs(Numerator), Denominator);
            Numerator /= gcd;
            Denominator /= gcd;
        }
        public static Fraction operator +(Fraction a, Fraction b)
            => new Fraction(a.Numerator * b.Denominator + b.Numerator * a.Denominator, a.Denominator * b.Denominator);
        public static Fraction operator -(Fraction a, Fraction b)
            => new Fraction(a.Numerator * b.Denominator - b.Numerator * a.Denominator, a.Denominator * b.Denominator);
        public static Fraction operator *(Fraction a, Fraction b)
            => new Fraction(a.Numerator * b.Numerator, a.Denominator * b.Denominator);
        public static Fraction operator /(Fraction a, Fraction b)
            => new Fraction(a.Numerator * b.Denominator, a.Denominator * b.Numerator);
        public static bool operator ==(Fraction a, Fraction b)
            => a.Numerator == b.Numerator && a.Denominator == b.Denominator;
        public static bool operator !=(Fraction a, Fraction b) => !(a == b);
        public override bool Equals(object obj) => obj is Fraction f && this == f;
        public override int GetHashCode() => HashCode.Combine(Numerator, Denominator);
        public bool IsZero => Numerator == 0;
        public bool IsOne => Numerator == 1 && Denominator == 1;
        public bool IsMinusOne => Numerator == -1 && Denominator == 1;
        public double ToDecimal() => (double)Numerator / Denominator;
        public string ToRawString() => Denominator == 1 ? $"{Numerator}" : $"{Numerator}/{Denominator}";
        public override string ToString()
        {
            if (Denominator == 1) return $"{Numerator}";
            if (Math.Abs(Numerator) < Denominator) return $"{Numerator}/{Denominator}";
            int whole = Numerator / Denominator;
            int remainder = Math.Abs(Numerator % Denominator);
            return remainder == 0 ? $"{whole}" : $"{whole} {remainder}/{Denominator}";
        }
        private static int GCD(int a, int b) { while (b != 0) { int t = b; b = a % b; a = t; } return a; }
    }
    public class Radical
    {
        public Fraction Coefficient { get; private set; }
        public int Radicand { get; private set; }
        public Radical(int coefficient, int radicand)
            : this(new Fraction(coefficient, 1), radicand) { }
        public Radical(Fraction coefficient, int radicand)
        {
            if (radicand < 0) throw new Exception($"Корень из отрицательного числа (√{radicand}) не определён в области вещественных чисел");
            Coefficient = coefficient;
            Radicand = radicand;
            Simplify();
        }
        private void Simplify()
        {
            if (Radicand == 0) { Coefficient = new Fraction(0, 1); return; }
            int outside = 1;
            int r = Radicand;
            for (int i = 2; i * i <= r; i++)
                while (r % (i * i) == 0) { r /= i * i; outside *= i; }
            Radicand = r;
            Coefficient = Coefficient * new Fraction(outside, 1);
        }
        public static Radical operator *(Radical a, Radical b)
            => new Radical(a.Coefficient * b.Coefficient, a.Radicand * b.Radicand);
        public double ToDecimal() => Coefficient.ToDecimal() * Math.Sqrt(Radicand);
        public override string ToString()
        {
            if (Coefficient.IsZero) return "0";
            if (Radicand == 1) return Coefficient.ToRawString();
            if (Coefficient.Denominator != 1)
            {
                int n = Coefficient.Numerator;
                int d = Coefficient.Denominator;
                string numPart = n == 1 ? "" : n == -1 ? "-" : n.ToString();
                return $"{numPart}√{Radicand}/{d}";
            }
            if (Coefficient.IsOne) return $"√{Radicand}";
            if (Coefficient.IsMinusOne) return $"-√{Radicand}";
            return $"{Coefficient.Numerator}√{Radicand}";
        }
    }
    public class MixedResult
    {
        public Fraction FractionPart { get; }
        public List<Radical> RadicalParts { get; }
        public MixedResult(Fraction f, List<Radical> radicals)
        {
            FractionPart = f;
            var dict = new Dictionary<int, Fraction>();
            foreach (var r in radicals)
            {
                if (dict.ContainsKey(r.Radicand))
                    dict[r.Radicand] = dict[r.Radicand] + r.Coefficient;
                else
                    dict[r.Radicand] = r.Coefficient;
            }
            RadicalParts = dict
                .Where(kvp => !kvp.Value.IsZero)
                .Select(kvp => new Radical(kvp.Value, kvp.Key))
                .ToList();
        }
        public double ToDecimal()
            => FractionPart.ToDecimal() + RadicalParts.Sum(r => r.ToDecimal());
        public override string ToString()
        {
            // Try to combine everything into a single fraction: (numerator terms)/denominator
            // Collect all denominators: FractionPart.Denominator and each Radical coefficient denominator
            int lcd = FractionPart.IsZero ? 1 : FractionPart.Denominator;
            foreach (var r in RadicalParts)
                lcd = LCM(lcd, r.Coefficient.Denominator);

            bool canCombine = lcd > 1 || (!FractionPart.IsZero && RadicalParts.Count > 0);
            if (canCombine && lcd > 1)
            {
                // Build numerator string: e.g. "68√3 - 5"
                var numParts = new List<string>();
                foreach (var r in RadicalParts)
                {
                    int scaledNum = r.Coefficient.Numerator * (lcd / r.Coefficient.Denominator);
                    string coeff = Math.Abs(scaledNum) == 1 ? "" : Math.Abs(scaledNum).ToString();
                    string sign = scaledNum < 0 ? "-" : (numParts.Count > 0 ? "+" : "");
                    string term = r.Radicand == 1
                        ? Math.Abs(scaledNum).ToString()
                        : $"{coeff}√{r.Radicand}";
                    numParts.Add(numParts.Count == 0
                        ? (scaledNum < 0 ? $"-{term}" : term)
                        : (scaledNum < 0 ? $" - {term}" : $" + {term}"));
                }
                if (!FractionPart.IsZero)
                {
                    int scaledFrac = FractionPart.Numerator * (lcd / FractionPart.Denominator);
                    string fracTerm = Math.Abs(scaledFrac).ToString();
                    numParts.Add(scaledFrac < 0
                        ? $" - {fracTerm}"
                        : (numParts.Count > 0 ? $" + {fracTerm}" : fracTerm));
                }
                string numeratorStr = string.Concat(numParts);
                return $"({numeratorStr})/{lcd}";
            }

            // Fallback: plain sum
            var parts = new List<string>();
            if (!FractionPart.IsZero)
                parts.Add(FractionPart.ToString());
            foreach (var r in RadicalParts)
            {
                string s = r.ToString();
                if (parts.Count > 0 && !s.StartsWith("-"))
                    parts.Add("+ " + s);
                else
                    parts.Add(s);
            }
            return parts.Count > 0 ? string.Join(" ", parts) : "0";
        }
        private static int LCM(int a, int b) => a / GCD(a, b) * b;
        private static int GCD(int a, int b) { while (b != 0) { int t = b; b = a % b; a = t; } return a; }
    }
    public static class MathNormalizer
    {
        public static object Normalize(object val)
        {
            if (val is Radical r)
            {
                if (r.Coefficient.IsZero) return new Fraction(0, 1);
                if (r.Radicand == 1) return r.Coefficient;
                return r;
            }
            if (val is MixedResult mr)
            {
                var frac = mr.FractionPart;
                var realRads = new List<Radical>();
                foreach (var rad in mr.RadicalParts)
                {
                    var n = Normalize(rad);
                    if (n is Fraction f) frac = frac + f;
                    else realRads.Add((Radical)n);
                }
                if (realRads.Count == 0) return frac;
                if (frac.IsZero && realRads.Count == 1) return realRads[0];
                return new MixedResult(frac, realRads);
            }
            return val;
        }
        public static double ToDecimal(object val)
        {
            val = Normalize(val);
            return val switch
            {
                Fraction f => f.ToDecimal(),
                Radical r => r.ToDecimal(),
                MixedResult mr => mr.ToDecimal(),
                _ => throw new Exception($"ToDecimal: неизвестный тип {val.GetType().Name}")
            };
        }
        public static List<object> GetTerms(object val)
        {
            val = Normalize(val);
            if (val is MixedResult mr)
            {
                var terms = new List<object>();
                if (!mr.FractionPart.IsZero) terms.Add(mr.FractionPart);
                terms.AddRange(mr.RadicalParts.Cast<object>());
                return terms;
            }
            return new List<object> { val };
        }
    }
    public static class Fmt
    {
        public static string Val(object v)
        {
            v = MathNormalizer.Normalize(v);
            return v switch
            {
                Fraction f => f.ToRawString(),
                Radical r => r.ToString(),
                MixedResult m => m.ToString(),
                _ => v.ToString()
            };
        }
        public static int LCM(int a, int b) => a / GCD(a, b) * b;
        public static int GCD(int a, int b) { while (b != 0) { int t = b; b = a % b; a = t; } return a; }
    }
    public static class MathOps
    {
        public static object Add(object left, object right)
        {
            left = MathNormalizer.Normalize(left);
            right = MathNormalizer.Normalize(right);
            if (left is Fraction fl && right is Fraction fr) return fl + fr;
            if (left is Radical rl && right is Radical rr)
            {
                if (rl.Radicand == rr.Radicand)
                    return MathNormalizer.Normalize(new Radical(rl.Coefficient + rr.Coefficient, rl.Radicand));
                return new MixedResult(new Fraction(0, 1), new List<Radical> { rl, rr });
            }
            if (left is Fraction f1 && right is Radical r1)
                return new MixedResult(f1, new List<Radical> { r1 });
            if (left is Radical r2 && right is Fraction f2)
                return new MixedResult(f2, new List<Radical> { r2 });
            Fraction fracSum = new Fraction(0, 1);
            var radicals = new Dictionary<int, Fraction>();
            void AddTerm(object t)
            {
                t = MathNormalizer.Normalize(t);
                if (t is Fraction f) fracSum = fracSum + f;
                else if (t is Radical r)
                {
                    if (radicals.ContainsKey(r.Radicand))
                        radicals[r.Radicand] = radicals[r.Radicand] + r.Coefficient;
                    else
                        radicals[r.Radicand] = r.Coefficient;
                }
            }
            foreach (var t in MathNormalizer.GetTerms(left)) AddTerm(t);
            foreach (var t in MathNormalizer.GetTerms(right)) AddTerm(t);
            var radList = radicals
                .Where(kvp => !kvp.Value.IsZero)
                .Select(kvp => new Radical(kvp.Value, kvp.Key))
                .ToList();
            if (radList.Count == 0) return fracSum;
            if (fracSum.IsZero && radList.Count == 1) return MathNormalizer.Normalize(radList[0]);
            return new MixedResult(fracSum, radList);
        }
        public static object Subtract(object left, object right) => Add(left, Negate(right));
        public static object Negate(object val)
        {
            val = MathNormalizer.Normalize(val);
            if (val is Fraction f) return new Fraction(-f.Numerator, f.Denominator);
            if (val is Radical r) return new Radical(new Fraction(-r.Coefficient.Numerator, r.Coefficient.Denominator), r.Radicand);
            if (val is MixedResult mr)
                return new MixedResult(
                    new Fraction(-mr.FractionPart.Numerator, mr.FractionPart.Denominator),
                    mr.RadicalParts.Select(r2 => new Radical(
                        new Fraction(-r2.Coefficient.Numerator, r2.Coefficient.Denominator),
                        r2.Radicand)).ToList());
            throw new Exception($"Нельзя отрицать тип {val.GetType().Name}");
        }
        public static object Multiply(object left, object right)
        {
            left = MathNormalizer.Normalize(left);
            right = MathNormalizer.Normalize(right);
            if (left is Fraction fl && right is Fraction fr) return fl * fr;
            if (left is Radical rl && right is Radical rr) return MathNormalizer.Normalize(rl * rr);
            if (left is Fraction f1 && right is Radical r1)
                return MathNormalizer.Normalize(new Radical(f1 * r1.Coefficient, r1.Radicand));
            if (left is Radical && right is Fraction) return Multiply(right, left);
            object result = new Fraction(0, 1);
            foreach (var lt in MathNormalizer.GetTerms(left))
                foreach (var rt in MathNormalizer.GetTerms(right))
                    result = Add(result, MultiplySimple(lt, rt));
            return result;
        }
        private static object MultiplySimple(object left, object right)
        {
            left = MathNormalizer.Normalize(left);
            right = MathNormalizer.Normalize(right);
            if (left is Fraction fl && right is Fraction fr) return fl * fr;
            if (left is Radical rl && right is Radical rr) return MathNormalizer.Normalize(rl * rr);
            if (left is Fraction f1 && right is Radical r1)
                return MathNormalizer.Normalize(new Radical(f1 * r1.Coefficient, r1.Radicand));
            if (left is Radical && right is Fraction) return MultiplySimple(right, left);
            throw new Exception($"MultiplySimple: неподдерживаемые типы {left.GetType().Name} и {right.GetType().Name}");
        }
        public static object Power(object baseVal, object expVal)
        {
            baseVal = MathNormalizer.Normalize(baseVal);
            expVal = MathNormalizer.Normalize(expVal);
            if (expVal is not Fraction exp)
                throw new Exception("Показатель степени должен быть дробью");
            int e = exp.Numerator;
            int q = exp.Denominator;
            if (e < 0) throw new Exception("Отрицательная степень пока не поддерживается");
            if (e == 0) return new Fraction(1, 1);
            if (q == 1)
            {
                if (baseVal is Fraction fb) return BinPow(fb, e);
                if (baseVal is Radical rb) return MathNormalizer.Normalize(BinPow(rb, e));
                if (baseVal is MixedResult) return BinPow(baseVal, e);
                throw new Exception($"Степень не поддерживается для типа {baseVal.GetType().Name}");
            }
            if (q == 2)
            {
                var powered = e == 1 ? baseVal : Power(baseVal, new Fraction(e, 1));
                powered = MathNormalizer.Normalize(powered);
                if (powered is Fraction fp)
                {
                    if (fp.Denominator == 1)
                        return MathNormalizer.Normalize(new Radical(1, fp.Numerator));
                    var numRad = MathNormalizer.Normalize(new Radical(1, fp.Numerator));
                    var denRad = MathNormalizer.Normalize(new Radical(1, fp.Denominator));
                    return Divide(numRad, denRad);
                }
                if (powered is Radical rp)
                    return MathNormalizer.Normalize(new Radical(rp.Coefficient, rp.Radicand * rp.Radicand));
            }
            // q > 2: не поддерживается в точной форме (потребовался бы тип "n-й корень")
            throw new Exception($"Степень {exp.ToRawString()} не поддерживается. Поддерживаются: целые степени и степень 1/2 (√)");
        }
        private static Fraction BinPow(Fraction b, int e)
        {
            var result = new Fraction(1, 1);
            while (e > 0)
            {
                if ((e & 1) == 1) result = result * b;
                b = b * b;
                e >>= 1;
            }
            return result;
        }
        private static Radical BinPow(Radical b, int e)
        {
            var result = new Radical(1, 1);
            while (e > 0)
            {
                if ((e & 1) == 1) result = result * b;
                b = b * b;
                e >>= 1;
            }
            return result;
        }
        private static object BinPow(object b, int e)
        {
            object result = new Fraction(1, 1);
            while (e > 0)
            {
                if ((e & 1) == 1) result = Multiply(result, b);
                b = Multiply(b, b);
                e >>= 1;
            }
            return result;
        }
        private static object RationalizeByConjugate(object numerator, object denominator, int maxDepth)
        {
            if (maxDepth == 0)
                throw new Exception("Не удалось рационализировать знаменатель: " +
                    Fmt.Val(MathNormalizer.Normalize(denominator)));
            var den = MathNormalizer.Normalize(denominator);
            if (den is Fraction fDen)
                return Divide(numerator, fDen);
            if (den is Radical rDen)
            {
                var num2 = Multiply(numerator, new Radical(1, rDen.Radicand));
                var den2 = rDen.Coefficient * new Fraction(rDen.Radicand, 1);
                return Divide(num2, den2);
            }
            if (den is MixedResult mrDen)
            {
                var conj = BuildConjugate(mrDen);
                var newNum = MathNormalizer.Normalize(Multiply(numerator, conj));
                var newDen = MathNormalizer.Normalize(Multiply(denominator, conj));
                return RationalizeByConjugate(newNum, newDen, maxDepth - 1);
            }
            throw new Exception($"Рационализация: неожиданный тип знаменателя {den.GetType().Name}");
        }
        private static object BuildConjugate(MixedResult mr)
        {
            var negRads = mr.RadicalParts
                .Select(r => new Radical(
                    new Fraction(-r.Coefficient.Numerator, r.Coefficient.Denominator),
                    r.Radicand))
                .ToList();
            var result = (object)new MixedResult(mr.FractionPart, negRads);
            return MathNormalizer.Normalize(result);
        }
        public static object Divide(object left, object right)
        {
            left = MathNormalizer.Normalize(left);
            right = MathNormalizer.Normalize(right);
            if (left is Fraction fl && right is Fraction fr)
                return fl / fr;
            if (right is Fraction fRight)
            {
                if (fRight.IsZero) throw new DivideByZeroException();
                return Multiply(left, new Fraction(fRight.Denominator, fRight.Numerator));
            }
            if (left is Radical rl && right is Radical rr)
            {
                if (rl.Radicand == rr.Radicand)
                    return rl.Coefficient / rr.Coefficient;
                var newCoeff = rl.Coefficient / (rr.Coefficient * new Fraction(rr.Radicand, 1));
                return MathNormalizer.Normalize(new Radical(newCoeff, rl.Radicand * rr.Radicand));
            }
            if (right is Radical rDiv)
            {
                var numerator = Multiply(left, new Radical(1, rDiv.Radicand));
                var denominator = rDiv.Coefficient * new Fraction(rDiv.Radicand, 1);
                return Divide(numerator, denominator);
            }
            if (right is MixedResult)
                return RationalizeByConjugate(left, right, maxDepth: 6);
            throw new Exception($"Деление не поддерживается для типов {left.GetType().Name} и {right.GetType().Name}");
        }
    }
    public enum TokenType { Fraction, Plus, Minus, Multiply, Divide, Power, Root, OpenParen, CloseParen }
    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }
        public Token(TokenType type, string value) { Type = type; Value = value; }
    }
    public class Lexer
    {
        public static List<Token> Tokenize(string input)
        {
            var raw = new List<Token>();
            input = input.Replace(" ", "").Replace(",", ".");
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (char.IsDigit(c) || c == '.')
                {
                    string number = c.ToString();
                    while (i + 1 < input.Length && (char.IsDigit(input[i + 1]) || input[i + 1] == '/' || input[i + 1] == '.'))
                    { i++; number += input[i]; }
                    raw.Add(new Token(TokenType.Fraction, number));
                }
                else if (c == '√')
                    raw.Add(new Token(TokenType.Root, "√"));
                else
                    raw.Add(c switch
                    {
                        '+' => new Token(TokenType.Plus, "+"),
                        '-' => new Token(TokenType.Minus, "-"),
                        '*' => new Token(TokenType.Multiply, "*"),
                        '/' => new Token(TokenType.Divide, "/"),
                        '^' => new Token(TokenType.Power, "^"),
                        '(' => new Token(TokenType.OpenParen, "("),
                        ')' => new Token(TokenType.CloseParen, ")"),
                        _ => throw new Exception($"Неизвестный символ: {c}")
                    });
            }
            return InsertImplicitMultiply(raw);
        }
        private static List<Token> InsertImplicitMultiply(List<Token> tokens)
        {
            var result = new List<Token>(tokens.Count * 2);
            for (int i = 0; i < tokens.Count; i++)
            {
                result.Add(tokens[i]);
                if (i + 1 >= tokens.Count) continue;
                var cur = tokens[i].Type;
                var next = tokens[i + 1].Type;
                bool leftIsValue = cur == TokenType.Fraction || cur == TokenType.CloseParen;
                bool rightIsStart = next == TokenType.Root || next == TokenType.OpenParen
                                                               || next == TokenType.Fraction;
                if (leftIsValue && rightIsStart)
                    result.Add(new Token(TokenType.Multiply, "*"));
            }
            return result;
        }
    }
    public abstract class Node
    {
        public abstract (object Value, List<string> Steps) EvaluateWithSteps();
        public object Evaluate() => EvaluateWithSteps().Value;
        public abstract string ToExprString();
    }
    public class FractionNode : Node
    {
        private readonly Fraction _value;
        public FractionNode(Fraction value) { _value = value; }
        public override (object, List<string>) EvaluateWithSteps() => (_value, new List<string>());
        public override string ToExprString() => _value.ToRawString();
    }
    public class RadicalNode : Node
    {
        private readonly Node _inner;
        private readonly string _displayStr;
        public RadicalNode(Node inner, string displayStr = null)
        {
            _inner = inner;
            _displayStr = displayStr;
        }
        public override (object, List<string>) EvaluateWithSteps()
        {
            var (innerVal, innerSteps) = _inner.EvaluateWithSteps();
            var steps = new List<string>(innerSteps);
            innerVal = MathNormalizer.Normalize(innerVal);
            if (innerVal is not Fraction f)
                throw new Exception("Под корнем должно быть рациональное число");
            if (f.IsZero) return (new Fraction(0, 1), steps);
            if (f.Numerator < 0) throw new Exception($"Корень из отрицательного числа (√{f.ToRawString()}) не определён в области вещественных чисел");
            string display = _displayStr ?? $"√({f.ToRawString()})";
            if (f.Denominator == 1)
            {
                var norm = MathNormalizer.Normalize(new Radical(1, f.Numerator));
                string simplified = Fmt.Val(norm);
                if (simplified != $"√{f.Numerator}")
                {
                    var radStep = StepBuilder.RadicalSimplify(f.Numerator, norm);
                    steps.Add(radStep ?? $"Упрощаем: {display} = {simplified}");
                }
                return (norm, steps);
            }
            // √(p/q) = √(p·q) / q  — rationalize fractional radicand
            int pq = f.Numerator * f.Denominator;
            var radical = MathNormalizer.Normalize(new Radical(1, pq));
            var result = MathOps.Divide(radical, new Fraction(f.Denominator, 1));
            result = MathNormalizer.Normalize(result);
            string radSimplified = Fmt.Val(radical);
            string radStep2 = StepBuilder.RadicalSimplify(pq, radical);
            if (radStep2 != null) steps.Add(radStep2);
            steps.Add($"√({f.ToRawString()}) = √({f.Numerator}·{f.Denominator})/{f.Denominator} = {radSimplified}/{f.Denominator} = {Fmt.Val(result)}");
            return (result, steps);
        }
        public override string ToExprString()
            => _displayStr ?? $"√({_inner.ToExprString()})";
    }
    public static class StepBuilder
    {
        // ── Radical simplification ──────────────────────────────────────────
        public static string RadicalSimplify(int radicand, object result)
        {
            if (radicand == 0) return null;
            int sq = LargestPerfectSquareFactor(radicand);
            if (sq == 1) return null;
            int outside = (int)Math.Round(Math.Sqrt(sq));
            int inside = radicand / sq;
            string res = Fmt.Val(result);
            string chain = inside == 1
                ? $"√{sq} = {outside}"
                : $"√({sq}·{inside}) = √{sq}·√{inside} = {outside}√{inside}";
            return $"Разложим {radicand} = {sq}·{inside} → √{radicand} = {chain} = {res}";
        }
        // ── Fraction + Fraction with different denominators ─────────────────
        public static string AddFractions(Fraction left, Fraction right, object result, string op)
        {
            int lcd = LCM(left.Denominator, right.Denominator);
            int lNum = left.Numerator * (lcd / left.Denominator);
            int rNum = right.Numerator * (lcd / right.Denominator);
            string res = Fmt.Val(result);
            return $"Приведём к общему знаменателю {lcd}: {lNum}/{lcd} {op} {rNum}/{lcd} = {res}";
        }
        // ── Like radicals: same radicand, integer coefficients ───────────────
        public static string AddLikeRadicals(Radical left, Radical right, object result)
        {
            string lC = left.Coefficient.ToRawString();
            string rC = right.Coefficient.ToRawString();
            string op = right.Coefficient.Numerator >= 0 ? "+" : "−";
            string rCA = right.Coefficient.Numerator >= 0 ? rC
                : new Fraction(-right.Coefficient.Numerator, right.Coefficient.Denominator).ToRawString();
            int sumN = left.Coefficient.Numerator + right.Coefficient.Numerator;
            string res = Fmt.Val(result);
            return $"Подобные корни (√{left.Radicand}): складываем коэффициенты {lC} {op} {rCA} = {sumN} → {res}";
        }
        // ── Like radicals: fractional coefficients need LCD ──────────────────
        public static string AddRadicalsWithFractionCoeffs(Radical left, Radical right, object result)
        {
            var lc = left.Coefficient;
            var rc = right.Coefficient;
            int lcd = LCM(lc.Denominator, rc.Denominator);
            int lNum = lc.Numerator * (lcd / lc.Denominator);
            int rNum = rc.Numerator * (lcd / rc.Denominator);
            string op = rNum >= 0 ? "+" : "−";
            int absR = Math.Abs(rNum);
            int sumNum = lNum + rNum;
            string res = Fmt.Val(result);
            if (lcd == 1)
                return $"Подобные корни (√{left.Radicand}): ({lNum} {op} {absR})·√{left.Radicand} = {res}";
            return $"Подобные корни (√{left.Radicand}): приводим коэффициенты к знаменателю {lcd}: "
                 + $"{lNum}/{lcd}·√{left.Radicand} {op} {absR}/{lcd}·√{left.Radicand} = {sumNum}/{lcd}·√{left.Radicand} = {res}";
        }
        // ── Fraction × Radical ────────────────────────────────────────────────
        public static string MultiplyFractionByRadical(Fraction f, Radical r, object result)
        {
            if (f.IsZero) return null;
            if (f.IsOne) return null;
            string res = Fmt.Val(result);
            string detail = f.Denominator != 1
                ? $"переносим {f.Denominator} в знаменатель → {res}"
                : $"= {res}";
            return $"Умножаем {f.ToRawString()} · √{r.Radicand}: {f.ToRawString()}·√{r.Radicand} — {detail}";
        }
        // ── Radical × Radical ─────────────────────────────────────────────────
        public static string MultiplyRadicals(Radical left, Radical right, object result)
        {
            string res = Fmt.Val(result);
            if (left.Radicand == right.Radicand)
                return $"√{left.Radicand} · √{left.Radicand} = {left.Radicand} → {res}";
            return $"√{left.Radicand} · √{right.Radicand} = √{left.Radicand * right.Radicand} → {res}";
        }
        // ── Mixed (radical + fraction) ────────────────────────────────────────
        public static string AddMixed(object left, object right, object result, string op)
        {
            string lStr = Fmt.Val(left);
            string rStr = Fmt.Val(right);
            string res = Fmt.Val(result);
            bool lRad = left is Radical || left is MixedResult;
            bool rRad = right is Radical || right is MixedResult;
            if (lRad && !rRad)
                return $"Дробная часть {rStr} не содержит корней — записываем отдельно: {res}";
            if (!lRad && rRad)
                return $"Целая часть {lStr} и корень {rStr} не приводятся — записываем: {res}";
            return $"{lStr} {op} {rStr} = {res}";
        }
        // ── Divide with irrational denominator ───────────────────────────────
        public static string DivideWithRationalization(object left, object right, object result)
        {
            string lStr = Fmt.Val(left);
            string rStr = Fmt.Val(right);
            string res = Fmt.Val(result);
            return $"Рационализируем знаменатель ({rStr}): умножаем числитель и знаменатель на сопряжённое → {res}";
        }
        // ── Power ─────────────────────────────────────────────────────────────
        public static string PowerStep(object baseVal, object expVal, object result)
        {
            string bStr = Fmt.Val(baseVal);
            string eStr = Fmt.Val(expVal);
            string res = Fmt.Val(result);
            if (baseVal is Radical rb && expVal is Fraction ef && ef.Denominator == 1)
            {
                int e = ef.Numerator;
                if (e % 2 == 0)
                    return $"({bStr})^{e}: чётная степень корня → {res}";
                return $"({bStr})^{e}: нечётная степень корня → {res}";
            }
            if (expVal is Fraction ef2 && ef2.Denominator == 2)
                return $"Степень 1/2 — это квадратный корень: √({bStr}) = {res}";
            return $"Возводим {bStr} в степень {eStr} = {res}";
        }
        // ── Helpers ───────────────────────────────────────────────────────────
        private static int LargestPerfectSquareFactor(int n)
        {
            int best = 1;
            for (int i = 2; (long)i * i <= n; i++)
                if (n % (i * i) == 0) best = i * i;
            return best;
        }
        private static int LCM(int a, int b) => a / GCD(a, b) * b;
        private static int GCD(int a, int b) { while (b != 0) { int t = b; b = a % b; a = t; } return a; }
    }
    public class BinaryNode : Node
    {
        public Node Left { get; }
        public Node Right { get; }
        public TokenType Operator { get; }
        public BinaryNode(Node left, Node right, TokenType op)
        { Left = left; Right = right; Operator = op; }
        public override (object Value, List<string> Steps) EvaluateWithSteps()
        {
            var (leftVal, leftSteps) = Left.EvaluateWithSteps();
            var (rightVal, rightSteps) = Right.EvaluateWithSteps();
            var steps = new List<string>();
            steps.AddRange(leftSteps);
            steps.AddRange(rightSteps);
            object result = Operator switch
            {
                TokenType.Plus => MathOps.Add(leftVal, rightVal),
                TokenType.Minus => MathOps.Subtract(leftVal, rightVal),
                TokenType.Multiply => MathOps.Multiply(leftVal, rightVal),
                TokenType.Divide => MathOps.Divide(leftVal, rightVal),
                TokenType.Power => MathOps.Power(leftVal, rightVal),
                _ => throw new Exception("Неверная операция")
            };
            result = MathNormalizer.Normalize(result);
            string lStr = Fmt.Val(leftVal);
            string rStr = Fmt.Val(rightVal);
            string res = Fmt.Val(result);
            string approxHint = (result is Radical || result is MixedResult)
                ? $" (≈ {MathNormalizer.ToDecimal(result):G6})"
                : "";
            switch (Operator)
            {
                case TokenType.Plus:
                    {
                        string step;
                        if (leftVal is Fraction lf && rightVal is Fraction rf && lf.Denominator != rf.Denominator)
                            step = StepBuilder.AddFractions(lf, rf, result, "+");
                        else if (leftVal is Radical rlp && rightVal is Radical rrp && rlp.Radicand == rrp.Radicand)
                        {
                            step = (rlp.Coefficient.Denominator != 1 || rrp.Coefficient.Denominator != 1)
                                ? StepBuilder.AddRadicalsWithFractionCoeffs(rlp, rrp, result)
                                : StepBuilder.AddLikeRadicals(rlp, rrp, result);
                        }
                        else
                            step = StepBuilder.AddMixed(leftVal, rightVal, result, "+")
                                ?? $"{lStr} + {rStr} = {res}";
                        steps.Add(step + approxHint);
                        break;
                    }
                case TokenType.Minus:
                    {
                        string step;
                        if (leftVal is Fraction lf2 && rightVal is Fraction rf2 && lf2.Denominator != rf2.Denominator)
                            step = StepBuilder.AddFractions(lf2, rf2, result, "−");
                        else if (leftVal is Radical rlm && rightVal is Radical rrm && rlm.Radicand == rrm.Radicand)
                        {
                            var negRrm = new Radical(new Fraction(-rrm.Coefficient.Numerator, rrm.Coefficient.Denominator), rrm.Radicand);
                            step = (rlm.Coefficient.Denominator != 1 || rrm.Coefficient.Denominator != 1)
                                ? StepBuilder.AddRadicalsWithFractionCoeffs(rlm, negRrm, result)
                                : StepBuilder.AddLikeRadicals(rlm, negRrm, result);
                        }
                        else
                            step = StepBuilder.AddMixed(leftVal, rightVal, result, "−")
                                ?? $"{lStr} − {rStr} = {res}";
                        steps.Add(step + approxHint);
                        break;
                    }
                case TokenType.Multiply:
                    {
                        string step;
                        if (leftVal is Fraction lf3 && rightVal is Radical rr3)
                            step = StepBuilder.MultiplyFractionByRadical(lf3, rr3, result)
                                ?? $"{lStr} × {rStr} = {res}";
                        else if (leftVal is Radical rl3 && rightVal is Fraction rf3)
                            step = StepBuilder.MultiplyFractionByRadical(rf3, rl3, result)
                                ?? $"{lStr} × {rStr} = {res}";
                        else if (leftVal is Radical mulL && rightVal is Radical mulR)
                            step = StepBuilder.MultiplyRadicals(mulL, mulR, result);
                        else
                            step = $"{lStr} × {rStr} = {res}";
                        steps.Add(step + approxHint);
                        break;
                    }
                case TokenType.Divide:
                    {
                        string step;
                        if (rightVal is MixedResult || rightVal is Radical)
                            step = StepBuilder.DivideWithRationalization(leftVal, rightVal, result);
                        else
                            step = $"{lStr} ÷ {rStr} = {res}";
                        steps.Add(step + approxHint);
                        break;
                    }
                case TokenType.Power:
                    {
                        string step = StepBuilder.PowerStep(leftVal, rightVal, result);
                        steps.Add(step + approxHint);
                        break;
                    }
            }
            return (result, steps);
        }
        public override string ToExprString()
        {
            string op = Operator switch
            {
                TokenType.Plus => "+",
                TokenType.Minus => "-",
                TokenType.Multiply => "×",
                TokenType.Divide => "÷",
                TokenType.Power => "^",
                _ => "?"
            };
            return $"{Left.ToExprString()} {op} {Right.ToExprString()}";
        }
    }
    public class PowerNode : Node
    {
        public Node BaseNode { get; }
        public Node ExponentNode { get; }
        public PowerNode(Node baseNode, Node exponentNode)
        { BaseNode = baseNode; ExponentNode = exponentNode; }
        public override (object Value, List<string> Steps) EvaluateWithSteps()
        {
            var (baseVal, baseSteps) = BaseNode.EvaluateWithSteps();
            var (expVal, expSteps) = ExponentNode.EvaluateWithSteps();
            var steps = new List<string>();
            steps.AddRange(baseSteps);
            steps.AddRange(expSteps);
            var result = MathNormalizer.Normalize(MathOps.Power(baseVal, expVal));
            steps.Add($"({Fmt.Val(baseVal)})^{Fmt.Val(expVal)} = {Fmt.Val(result)}");
            return (result, steps);
        }
        public override string ToExprString()
            => $"({BaseNode.ToExprString()})^{ExponentNode.ToExprString()}";
    }
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _pos;
        private Token Current => _pos < _tokens.Count ? _tokens[_pos] : null;
        public Parser(List<Token> tokens) { _tokens = tokens; _pos = 0; }
        public Node Parse()
        {
            var node = ParseExpression();
            if (_pos < _tokens.Count)
                throw new Exception($"Лишние символы в конце выражения: «{_tokens[_pos].Value}»");
            return node;
        }
        public Node ParseExpression() => ParseAddSubtract();
        private Node ParseAddSubtract()
        {
            Node node = ParseMultiplyDivide();
            while (Current != null && (Current.Type == TokenType.Plus || Current.Type == TokenType.Minus))
            {
                var op = Current.Type; _pos++;
                node = new BinaryNode(node, ParseMultiplyDivide(), op);
            }
            return node;
        }
        private Node ParseMultiplyDivide()
        {
            Node node = ParsePower();
            while (Current != null && (Current.Type == TokenType.Multiply || Current.Type == TokenType.Divide))
            {
                var op = Current.Type; _pos++;
                node = new BinaryNode(node, ParsePower(), op);
            }
            return node;
        }
        private Node ParsePower()
        {
            Node node = ParsePrimary();
            if (Current != null && Current.Type == TokenType.Power)
            {
                _pos++;
                Node exponent = ParsePower();
                node = new PowerNode(node, exponent);
            }
            return node;
        }
        private Node ParsePrimary()
        {
            if (Current == null) throw new Exception("Конец выражения");
            if (Current.Type == TokenType.Minus)
            {
                _pos++;
                return new BinaryNode(
                    new FractionNode(new Fraction(0, 1)),
                    ParsePrimary(),
                    TokenType.Minus);
            }
            if (Current.Type == TokenType.Root)
            {
                _pos++;
                if (Current != null && Current.Type == TokenType.OpenParen)
                {
                    _pos++;
                    var innerNode = ParseExpression();
                    if (Current == null || Current.Type != TokenType.CloseParen)
                        throw new Exception("Ожидалась ')' после выражения под корнем");
                    _pos++;
                    return new RadicalNode(innerNode, $"√({innerNode.ToExprString()})");
                }
                if (Current == null || Current.Type != TokenType.Fraction)
                    throw new Exception("После √ должно быть число или выражение в скобках");
                int rad = int.Parse(Current.Value);
                _pos++;
                return new RadicalNode(new FractionNode(new Fraction(rad, 1)), $"√{rad}");
            }
            if (Current.Type == TokenType.Fraction)
            {
                var val = new Fraction(Current.Value);
                _pos++;
                return new FractionNode(val);
            }
            if (Current.Type == TokenType.OpenParen)
            {
                _pos++;
                var node = ParseExpression();
                if (Current == null || Current.Type != TokenType.CloseParen)
                    throw new Exception("Ожидалась закрывающая скобка");
                _pos++;
                return node;
            }
            throw new Exception("Неожиданный токен: " + Current.Value);
        }
    }
    public class UnivCalc
    {
        // Returns numbered steps + exact result string
        public static (List<string> Steps, string Result) Calculate(string input)
        {
            var (steps, result, _) = CalculateDetailed(input);
            return (steps, result);
        }
        // Full output: numbered steps, exact symbolic result, decimal approximation
        public static (List<string> Steps, string Exact, string Approx) CalculateDetailed(string input)
        {
            var tokens = Lexer.Tokenize(input);
            var parser = new Parser(tokens);
            var root = parser.Parse();
            var (evaluated, rawSteps) = root.EvaluateWithSteps();
            evaluated = MathNormalizer.Normalize(evaluated);
            var seen = new HashSet<string>();
            var steps = new List<string>();
            foreach (var s in rawSteps)
                if (!string.IsNullOrWhiteSpace(s) && seen.Add(s))
                    steps.Add(s);
            // Number each step with a descriptive label
            var numbered = new List<string>();
            foreach (var (s, i) in steps.Select((s, i) => (s, i)))
                numbered.Add($"Шаг {i + 1}: {s}");
            string exact = evaluated switch
            {
                Fraction f => f.ToString(),
                Radical r => r.ToString(),
                MixedResult mr => mr.ToString(),
                _ => evaluated.ToString()
            };
            string approx = null;
            if (evaluated is Radical || evaluated is MixedResult)
            {
                double d = MathNormalizer.ToDecimal(evaluated);
                string rounded = Math.Round(d, 6).ToString("G10")
                    .TrimEnd('0').TrimEnd('.');
                approx = $"≈ {rounded}";
            }
            return (numbered, exact, approx);
        }
        public static (List<string> Steps, string Exact, string Approx) CalculateWithApprox(string input, int decimals = 6)
            => CalculateDetailed(input);
    }
}