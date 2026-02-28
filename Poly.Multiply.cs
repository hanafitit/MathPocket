using System;
using System.Collections.Generic;

namespace MathPocket
{
    // ═══════════════════════════════════════════════════════════════
    //  Умножение многочленов — в разработке
    //  Заглушки позволяют боту запускаться, пока функции не готовы.
    // ═══════════════════════════════════════════════════════════════

    public abstract class StubFunction : FunctionBase
    {
        protected abstract string SectionName { get; }
        public override string[] Parameters => [];
        public override double Calculate(double[] inputs) => throw new NotSupportedException();

        public override InputStep[] Steps =>
        [
            new InputStep
            {
                Question  = $"⚙️ Раздел «{SectionName}» пока в разработке.\n\nНажми «◀️ Назад».",
                Validate  = _ => "Эта функция ещё не реализована. Нажми «◀️ Назад»."
            }
        ];

        public override string CalculateFromAnswers(List<string> answers) =>
            $"⚙️ «{Name}» ещё не реализована.";
    }

    public class PolyTimesMonomial    : StubFunction { public override string Name => "Многочлен × одночлен";      protected override string SectionName => "Умножение многочленов"; }
    public class PolyTimesPolyFunction: StubFunction { public override string Name => "Многочлен × многочлен";     protected override string SectionName => "Умножение многочленов"; }
    public class PolySimplifyExpression: StubFunction { public override string Name => "Упростить выражение";      protected override string SectionName => "Умножение многочленов"; }
    public class PolyEvalProduct      : StubFunction { public override string Name => "Вычислить произведение";    protected override string SectionName => "Умножение многочленов"; }
    public class PolyEquation         : StubFunction { public override string Name => "Уравнение с многочленами";  protected override string SectionName => "Умножение многочленов"; }
    public class PolyProveIdentity    : StubFunction { public override string Name => "Доказать тождество";        protected override string SectionName => "Умножение многочленов"; }
}
