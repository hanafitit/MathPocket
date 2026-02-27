namespace MathPocket
{
    // ═══════════════════════════════════════════════════════════════
    //  Каталог всех разделов и функций бота
    //
    //  Чтобы добавить функцию:
    //    1. Создайте класс, унаследованный от FunctionBase
    //    2. Добавьте new МойКласс() в нужный MathSection ниже
    //
    //  Чтобы добавить новый раздел:
    //    Добавьте new MathCategory { ... } в массив All
    //
    //  Разделы с пустым Functions[] показывают «пока пуст».
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
                        Name = "✖️ Умножение многочленов",
                        Functions = new FunctionBase[]
                        {
                            
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
