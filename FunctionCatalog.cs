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
                            new IsLinearFunction(),             // 1. понятие
                            new LinearPlotFunction(),           // 2. построить график — основа
                            new LinearEvalFunction(),           // 3. найти y/x по формуле
                            new LinearPointBelongsFunction(),   // 4. точка на графике
                            new LinearSignFunction(),           // 5. знак функции
                            new LinearFindKFunction(),          // 6. найти k
                            new LinearFindBFunction(),          // 7. найти b
                        ]
                    },
                    new()
                    {
                        Name = "🔀 Взаимное расположение функций",
                        Functions =
                        [
                            new LinearRelationFunction(),               // 1. теория: виды расположения
                            new LinearTwoGraphsFunction(),              // 2. построить два графика — наглядность
                            new LinearParallelExamplesFunction(),       // 3. примеры параллельных
                            new LinearWriteRelatedFunction(),           // 4. написать формулу параллельной
                            new LinearIntersectionPointFunction(),      // 5. найти точку пересечения
                            new LinearProveIntersectFunction(),         // 6. доказать пересечение
                            new LinearQuadrantsFunction(),              // 7. в каких четвертях
                            new LinearFindBFromIntersectionFunction(),  // 8. найти b из общей точки
                            new LinearFindByPointOnOyFunction(),        // 9. функция по точке на Oy
                            new LinearParallelThroughPointFunction(),   // 10. параллельная через точку
                            new LinearFindFormulaByPointAndOyFunction(),// 11. формула по точке и Oy
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

            new()
            {
                Name = "📉 Функция y = k/x (гипербола)",
                Functions =
                [
                    new HyperbolaPointBelongsFunction(),
                    new HyperbolaPlotFunction(),
                    new HyperbolaTwoGraphsFunction(),
                    new HyperbolaTableFunction(),
                    new HyperbolaRootsFunction(),
                    new HyperbolaSolveGraphicallyFunction(),
                    new HyperbolaIntersectFunction(),
                    new HyperbolaCanIntersectFunction(),
                    new HyperbolaAbsFunction(),
                    new HyperbolaMinMaxFunction(),
                    new HyperbolaFindKFunction(),
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
            new()
            {
                Name = "📊 Элементы статистики",
                SubSections =
                [
                    new()
                    {
                        Name = "📋 Вариационные ряды",
                        Functions =
                        [
                            new PopulationAndSampleFunction(),
                            new IsVariationRowFunction(),
                            new MakeVariationRowFunction(),
                            new MinMaxVariantFunction(),
                            new FrequencyVariantFunction(),
                            new VariationRowFromTableFunction(),
                            new VariationRowAnalysisFunction(),
                            new FillTableThreeFunctionsFunction(),
                        ]
                    },
                    new()
                    {
                        Name = "📊 Абсолютная и относительная частота",
                        Functions =
                        [
                            new AbsRelFrequencyFunction(),
                            new FrequencyTableFunction(),
                            new SpecificVariantFrequencyFunction(),
                            new StatRowToFreqTableFunction(),
                            new FindRelFreqFromTableFunction(),
                            new HomeworkFrequencyFunction(),
                        ]
                    },
                ]
            },
        ];
    }
}
