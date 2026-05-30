namespace MathPocket
{
    internal static class FunctionCatalog
    {
        public static readonly MathCategory[] All =
        [
            new()
            {
                Name = "⚡ Степени и основания",
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
                        Name = "🔣 Что такое многочлен",
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
                        Name = "➕ Сложение и вычитание многочленов",
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
                        Name = "➗ Деление многочлена на одночлен",
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
                        Name = "🔄 Упрощение и тождества",
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
                        Name = "📌 Что такое функция",
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
                        Name = "📊 Функция задана таблицей",
                        Functions =
                        [
                            new TableAnalysisFunction(),
                            new DetectFormulaFunction(),
                        ]
                    },
                    new()
                    {
                        Name = "📏 Линейная функция y = kx + b",
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
                        Name = "🔀 Две прямые: параллельность и пересечение",
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
                        Name = "📐 Система уравнений (по графику)",
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
                        Name = "📐 Парабола y = ax²",
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
                    new()
                    {
                        Name = "📉 Гипербола y = k/x",
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
                ]
            },

            new()
            {
                Name = "✂️ Формулы сокращённого умножения (ФСУ)",
                SubSections =
                [
                    new()
                    {
                        Name = "➖ Разность квадратов: (a+b)(a−b) = a²−b²",
                        Functions =
                        [
                            new DiffSqExpandFunction(),
                            new DiffSqFactorFunction(),
                            new DiffSqComputeFunction(),
                            new DiffSqProductTrickFunction(),
                            new DiffSqSimplifyFunction(),
                            new DiffSqEvalFunction(),
                            new DiffSqEquationFunction(),
                            new DiffSqProveIdentityFunction(),
                        ]
                    },
                    new()
                    {
                        Name = "🔲 Квадрат суммы и разности: (a±b)²",
                        Functions =
                        [
                            new SqSumExpandFunction(),
                            new SqSumComputeFunction(),
                            new SqSumRecognizeFunction(),
                            new SqSumSimplifyFunction(),
                            new SqSumFactorFunction(),
                            new SqSumAdvancedSimplifyFunction(),
                            new SqSumPolyToSquareFunction(),
                            new SqSumFactorPolyFunction(),
                            new SqSumEquationFunction(),
                            new SqSumInequalityFunction(),
                            new SqSumProveSignFunction(),
                        ]
                    },
                    new()
                    {
                        Name = "🔷 Куб суммы и разности: (a±b)³",
                        Functions =
                        [
                            new CubeSumExpandFunction(),
                            new CubeSumRecognizeFunction(),
                            new CubeSumSimplifyEvalFunction(),
                            new CubeSumEquationFunction(),
                            new CubeSumProveIdentityFunction(),
                            new CubeSumSimplifyFunction(),
                            new CubeSumEquationAdvancedFunction(),
                            new CubeSumInequalityFunction(),
                            new CubeSumProveZeroFunction(),
                        ]
                    },
                    new()
                    {
                        Name = "🔶 Сумма и разность кубов: a³±b³",
                        Functions =
                        [
                            new CubeSumFactorFunction(),
                            new CubeSumProductExpandFunction(),
                            new CubeSumSimplify2Function(),
                            new CubeSumFactorEquationFunction(),
                            new CubeSumFactorInequalityFunction(),
                            new CubeSumFactorProveFunction(),
                            new CubeSumWriteAsProductFunction(),
                            new CubeSumAllFSUSimplifyFunction(),
                            new CubeSumAdvancedEquationFunction(),
                            new CubeSumAllFSUProveFunction(),
                        ]
                    },
                    new()
                    {
                        Name = "⚗️ Все ФСУ вместе",
                        Functions =
                        [
                            new IdentitySimplify2Function(),
                            new IdentityEquationFunction(),
                            new IdentityFindRootsFunction(),
                            new IdentityProductZeroFunction(),
                            new IdentityComplexEquationFunction(),
                            new IdentityInequalityFunction(),
                            new IdentityProveAllFSUFunction(),
                            new IdentityAdvancedSimplifyFunction(),
                            new IdentityHighDegreeEquationFunction(),
                            new IdentityAdvancedInequalityFunction(),
                            new IdentityIntegerSolutionFunction(),
                            new IdentityProveAdvancedFunction(),
                        ]
                    },
                ]
            },
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
                Name = "📊 Статистика",
                SubSections =
                [
                    new()
                    {
                        Name = "📋 Упорядоченные ряды данных",
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
                        Name = "📊 Частота появления значения",
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
