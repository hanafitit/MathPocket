using System;
using System.Collections.Generic;

namespace MathPocket
{
    public sealed record InputStep
    {
        public required string Question { get; init; }
        public Func<string, string?> Validate { get; init; } = _ => null;
    }

    public sealed class StepInputSession
    {
        public int CurrentStep { get; set; } = 0;
        public List<string> Answers { get; } = [];
    }
}
