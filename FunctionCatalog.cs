namespace MathPocket
{
    internal static class FunctionCatalog
    {
        public static readonly MathCategory[] All =
        [
            new()
            {
                Name = "⚡ Степень",
                Functions =
                [
                    new PowerFunction(),
                    new EvaluateAtValueFunction(),
                    new PowerProductFunction(),
                    new PowerQuotientFunction(),
                    new PowerOfPowerFunction(),
                    new PowerOfProductFunction(),
                    new PowerOfFractionFunction(),
                    new ComparePowersFunction(),
                    new FindBaseOrExponentFunction(),
                ]
            },

            new()
            {
                Name = "🔢 Одночлены",
                Functions =
                [
                    new MonomialStandardFormFunction(),
                    new MonomialPowerFunction(),
                    new MonomialMultiplyFunction(),
                    new MonomialDivideFunction(),
                    new MonomialDivideEvalFunction(),
                ]
            },

            new()
            {
                Name = "🔣 Многочлены",
                SubSections =
                [
                    new()
                    {
                        Name = "🔣 Основы многочленов",
                        Functions =
                        [
                            new PolynomialFromMonomialsFunction(),
                            new PolynomialStandardMembersFunction(),
                            new PolynomialNameMembersFunction(),
                            new PolynomialDegreeFunction(),
                            new PolynomialStandardFormFunction(),
                            new PolynomialValueFunction(),
                            new PolynomialValueTwoVarsFunction(),
                        ]
                    },
                    new()
                    {
                        Name = "➕ Сложение и вычитание",
                        Functions =
                        [
                            new PolynomialLikeTermsFunction(),
                            new PolynomialAddFunction(),
                            new PolynomialSubtractFunction(),
                        ]
                    },
                    new()
                    {
                        Name = "✖️ Умножение многочленов",
                        Functions =
                        [
                            new PolyTimesMonomial(),
                            new PolyTimesPolyFunction(),
                            new PolySimplifyExpression(),
                            new PolyEvalProduct(),
                            new PolyEquation(),
                            new PolyProveIdentity(),
                        ]
                    },
                    new()
                    {
                        Name = "➗ Деление на одночлен",
                        Functions =
                        [
                            new MonomialDividePolyFunction(),
                            new PolynomialDivideByMonomialFunction(),
                            new PolyDivideSimplifyFunction(),
                            new PolyDivideEvalFunction(),
                        ]
                    },
                    new()
                    {
                        Name = "🔧 Разложение на множители",
                        Functions =
                        [
                            new FactorOutGcfFunction(),
                            new FactorOutPolyFunction(),
                            new FactorEquationFunction(),
                            new GroupingFourTermsFunction(),
                            new GroupingSixTermsFunction(),
                            new GroupingEquationFunction(),
                        ]
                    },
                    new()
                    {
                        Name = "🔄 Тождественные преобразования",
                        Functions =
                        [
                            new IdentitySimplifyFunction(),
                            new IdentitySumAsProductFunction(),
                            new IdentityInequalityIntegerFunction(),
                        ]
                    },
                    new()
                    {
                        Name = "✅ Проверь себя",
                        Functions =
                        [
                            new PolyDivideByMonomialFunction(),
                            new PolyFindEqualXFunction(),
                            new PolyFindGcmExpressionFunction(),
                        ]
                    },
                ]
            },

            new()
            {
                Name = "📈 Функции и графики",
                SubSections =
                [
                    new()
                    {
                        Name = "📌 Понятие функции",
                        Functions =
                        [
                            new IsFunctionFunction(),
                            new DomainFunction(),
                            new DomainFromTableFunction(),
                            new GraphDomainFunction(),
                            new IsIncreasingDecreasingFunction(),
                            new FormulaFromTableFunction(),
                            new TableFromFormulaFunction(),
                        ]
                    },
                    new()
                    {
                        Name = "📊 Табличная функция",
                        Functions =
                        [
                            new TableAnalysisFunction(),
                            new DetectFormulaFunction(),
                        ]
                    },
                    new()
                    {
                        Name = "📏 Линейная функция и её график",
                        Functions =
                        [
                            new IsLinearFunction(),
                            new LinearEvalFunction(),
                            new LinearPointBelongsFunction(),
                            new LinearFindBFunction(),
                            new LinearFindKFunction(),
                            new LinearPlotFunction(),
                            new LinearSignFunction(),
                        ]
                    },
                    new()
                    {
                        Name = "🔀 Взаимное расположение функций",
                        Functions =
                        [
                            new LinearRelationFunction(),
                            new LinearIntersectionPointFunction(),
                            new LinearProveIntersectFunction(),
                            new LinearWriteRelatedFunction(),
                            new LinearParallelExamplesFunction(),
                            new LinearTwoGraphsFunction(),
                            new LinearQuadrantsFunction(),
                            new LinearFindBFromIntersectionFunction(),
                            new LinearFindByPointOnOyFunction(),
                            new LinearParallelThroughPointFunction(),
                            new LinearFindFormulaByPointAndOyFunction(),
                        ]
                    },
                    new()
                    {
                        Name = "📐 Системы уравнений (графический способ)",
                        Functions =
                        [
                            new SystemGraphOxFunction(),
                            new SystemGraphOyFunction(),
                            new SystemGraphPlotFunction(),
                            new SystemSolveGraphicallyFunction(),
                            new SystemCountSolutionsFunction(),
                            new SystemEvalExpressionFunction(),
                        ]
                    },
                    new()
                    {
                        Name = "📐 Функция y = ax²",
                        Functions =
                        [
                            new QuadraticPointBelongsFunction(),
                            new QuadraticPlotFunction(),
                            new QuadraticTwoGraphsFunction(),
                            new QuadraticCompareValuesFunction(),
                            new QuadraticRootsCountFunction(),
                            new QuadraticIntersectsLinearFunction(),
                            new QuadraticFindRootsFunction(),
                            new QuadraticMonotonicFunction(),
                            new QuadraticMinMaxFunction(),
                            new QuadraticCanIntersectFunction(),
                        ]
                    },
                ]
            },

            new() { Name = "✂️ Формулы сокращённого умножения" },
            new() { Name = "➗ Алгебраические дроби" },
            new() { Name = "√ Квадратные корни" },
            new() { Name = "🔲 Квадратные уравнения" },
            new() { Name = "⚖️ Неравенства" },
            new() { Name = "🔀 Системы уравнений и неравенств" },
            new() { Name = "🎲 Комбинаторика" },
            new() { Name = "🔢 Последовательности" },
            new() { Name = "📐 Тригонометрия" },
            new()
            {
                Name = "🎯 Теория вероятностей",
                Functions = [new PercentOfNumberFunction()]
            },
            new() { Name = "📊 Элементы статистики" },
        ];
    }
}
