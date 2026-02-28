namespace MathPocket
{
    // ═══════════════════════════════════════════════════════════════
    //  Каталог всех разделов и функций бота
    // ═══════════════════════════════════════════════════════════════

    internal static class FunctionCatalog
    {
        public static readonly MathCategory[] All = new MathCategory[]
        {
            // ── ⚡ Степень ────────────────────────────────────────
            new MathCategory
            {
                Name = "⚡ Степень",
                Functions = new FunctionBase[]
                {
                    new PowerFunction(),
                    new EvaluateAtValueFunction(),
                    new PowerProductFunction(),
                    new PowerQuotientFunction(),
                    new PowerOfPowerFunction(),
                    new PowerOfProductFunction(),
                    new PowerOfFractionFunction(),
                    new ComparePowersFunction(),
                    new FindBaseOrExponentFunction(),
                }
            },

            // ── 🔢 Одночлены ──────────────────────────────────────
            new MathCategory
            {
                Name = "🔢 Одночлены",
                Functions = new FunctionBase[]
                {
                    new MonomialStandardFormFunction(),
                    new MonomialPowerFunction(),
                    new MonomialMultiplyFunction(),
                    new MonomialDivideFunction(),
                    new MonomialDivideEvalFunction(),
                }
            },

            // ── 🔣 Многочлены (с подразделами) ────────────────────
            new MathCategory
            {
                Name = "🔣 Многочлены",
                SubSections = new MathSection[]
                {
                    new MathSection
                    {
                        Name = "🔣 Основы многочленов",
                        Functions = new FunctionBase[]
                        {
                            new PolynomialFromMonomialsFunction(),
                            new PolynomialStandardMembersFunction(),
                            new PolynomialNameMembersFunction(),
                            new PolynomialDegreeFunction(),
                            new PolynomialStandardFormFunction(),
                            new PolynomialValueFunction(),
                            new PolynomialValueTwoVarsFunction(),
                        }
                    },
                    new MathSection
                    {
                        Name = "➕ Сложение и вычитание",
                        Functions = new FunctionBase[]
                        {
                            new PolynomialLikeTermsFunction(),
                            new PolynomialAddFunction(),
                            new PolynomialSubtractFunction(),
                        }
                    },
                    new MathSection
                    {
                        Name = "✖️ Умножение многочленов",
                        Functions = new FunctionBase[]
                        {
                            new PolyTimesMonomial(),
                            new PolyTimesPolyFunction(),
                            new PolySimplifyExpression(),
                            new PolyEvalProduct(),
                            new PolyEquation(),
                            new PolyProveIdentity(),
                        }
                    },
                    new MathSection
                    {
                        Name = "➗ Деление на одночлен",
                        Functions = new FunctionBase[]
                        {
                            new MonomialDividePolyFunction(),
                            new PolynomialDivideByMonomialFunction(),
                            new PolyDivideSimplifyFunction(),
                            new PolyDivideEvalFunction(),
                        }
                    },
                    new MathSection
                    {
                        Name = "🔧 Разложение на множители",
                        Functions = new FunctionBase[]
                        {
                            new FactorOutGcfFunction(),
                            new FactorOutPolyFunction(),
                            new FactorEquationFunction(),
                            new GroupingFourTermsFunction(),
                            new GroupingSixTermsFunction(),
                            new GroupingEquationFunction(),
                        }
                    },
                    new MathSection
                    {
                        Name = "🔄 Тождественные преобразования",
                        Functions = new FunctionBase[]
                        {
                            new IdentitySimplifyFunction(),
                            new IdentitySumAsProductFunction(),
                            new IdentityInequalityIntegerFunction(),
                        }
                    },
                    new MathSection
                    {
                        Name = "✅ Проверь себя",
                        Functions = new FunctionBase[]
                        {
                            new PolyDivideByMonomialFunction(),
                            new PolyFindEqualXFunction(),
                            new PolyFindGcmExpressionFunction(),
                        }
                    },
                }
            },

            // ── 📈 Графики ────────────────────────────────────────
            new MathCategory
            {
                Name = "📈 Графики",
                SubSections = new MathSection[]
                {
                    new MathSection
                    {
                        Name = "📌 Понятие функции",
                        Functions = new FunctionBase[]
                        {
                            new DomainFunction(),
                            new DomainFromTableFunction(),
                            new IsIncreasingDecreasingFunction(),
                            new FormulaFromTableFunction(),
                            new TableFromFormulaFunction(),
                        }
                    },
                    new MathSection
                    {
                        Name = "📊 Табличная функция",
                        Functions = new FunctionBase[]
                        {
                            new TableAnalysisFunction(),
                            new DetectFormulaFunction(),
                        }
                    },
                }
            },

            // ── В разработке ──────────────────────────────────────
            new MathCategory { Name = "✂️ Формулы сокращённого умножения" },
            new MathCategory { Name = "➗ Алгебраические дроби" },
            new MathCategory { Name = "√ Квадратные корни" },
            new MathCategory { Name = "🔲 Квадратные уравнения" },
            new MathCategory { Name = "⚖️ Неравенства" },
            new MathCategory { Name = "🔀 Системы уравнений и неравенств" },
            new MathCategory { Name = "🎲 Комбинаторика" },
            new MathCategory { Name = "🔢 Последовательности" },
            new MathCategory { Name = "📐 Тригонометрия" },
            new MathCategory
            {
                Name = "🎯 Теория вероятностей",
                Functions = new FunctionBase[] { new PercentOfNumberFunction() }
            },
            new MathCategory { Name = "📊 Элементы статистики" },
        };
    }
}
