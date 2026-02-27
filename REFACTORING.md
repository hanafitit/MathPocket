# Рефакторинг MathPocket — Что изменилось

## FunctionBase.cs
- Удалён дублирующийся `using System;`
- Тело класса разделено на логические секции с комментариями
- `Parameters` и `Keywords` получили значения по умолчанию через `Array.Empty<string>()` — убрано дублирование в каждом наследнике
- `CalculateFromText` теперь использует `CultureInfo.InvariantCulture` явно
- Весь `partial` код пошагового ввода перенесён из `StepInput.cs` — нет больше split partial

## StepInput.cs
- `InputStep` и `StepInputSession` получили `sealed`
- `InputStep.Question` теперь `required` — нельзя создать шаг без вопроса (compile-time safety)
- Код partial `FunctionBase` удалён отсюда и перенесён в `FunctionBase.cs`

## BotHandler.cs
- Магические строки кнопок (`"◀️ Назад"` и др.) вынесены в `const` поля — нет риска опечаток
- `HandleBack` разбит на `HandleBackFromInput` и `HandleBackFromFunction` — каждый метод делает одно дело
- `HandleInputData` разбит на `HandleStepInput`, `FinishStepSession`, `HandleSingleLineInput`
- Строение строки ответа калькулятора переведено на `StringBuilder` вместо `+=`
- `ConcurrentDictionary` поля переименованы в `_camelCase` (соответствие C# конвенциям)
- `MathCategory` и `MathSection` получили `sealed` (не предполагается наследование)
- `MathCategory.Functions.Any()` заменено на `Functions.Length == 0` (нет LINQ-аллокации)
- `BotHandler` получил `sealed internal` (нет смысла наследоваться)

## Program.cs
- Хардкоженный токен удалён — теперь бросает `InvalidOperationException` если env не задан
- Обработчик ошибок polling вынесен в отдельный `HandlePollingError`
- Веб-сервер вынесен в отдельный `RunWebServer` (не локальная функция)
- `Encoding.UTF8.GetBytes("Бот работает!")` → `"OK"` (лаконичнее)
- `TaskCanceledException` заменён на `OperationCanceledException` (базовый тип)

## MonomialFunctions.cs
- Добавлен `MonomialSteps` — статический класс с общими шагами (`Coeff`, `DegreeA`, `DegreeB`)
  Все три функции переиспользуют их через `with` expression вместо копипаста
- `using System.Globalization` вынесен на уровень файла — убраны дублирующиеся обращения
- `Monomial.ParseDouble` / `ParseInt` убраны вызовы инлайн, читаемость выросла
- `Array.Empty<string>()` → `[]` (C# 12 collection expressions)
- Комментарии `// Шаг 0 —` убраны там, где шаги говорят сами за себя

## UniversalCalculator.cs
- **`MathUtils`** — новый класс с `GCD` и `LCM`. Удалены 4 копии в `Fraction`, `MixedResult`, `Fmt`, `StepBuilder`
- `Fraction`, `Radical`, `MixedResult`, `Token`, `Parser`, `PowerNode`, `BinaryNode`, `RadicalNode`, `FractionNode` получили `sealed`
- `BinaryNode.EvaluateWithSteps` — гигантский `switch` на строчки вынесен в `BuildStep(...)` — читается линейно
- Инициализация `List<string>` с прегрузом: `new List<string>(leftSteps.Count + rightSteps.Count + 1)` — меньше реаллокаций
- `Exception` заменены на `InvalidOperationException` / `ArgumentException` где уместно
- `Lexer.Tokenize` — цикл переписан с явным `continue` вместо вложенных `if`/`else`
- `UnivCalc.CalculateWithApprox` удалён (дублировал `CalculateDetailed`)
- `Fmt` потерял `LCM`/`GCD` — они перенесены в `MathUtils`

## PolynomialFunctions.cs
- Без изменений в этом PR. Следующий шаг: вынести 4 копии `private static long GCD(long, long)` в `PolyMath` утилиту

## FunctionCatalog.cs / Material.cs
- `Material` получил `sealed`, `init` свойства, `[]` вместо `Array.Empty<string>()`
- Без изменений в каталоге

---

## TODO для следующего рефакторинга

1. **`PolynomialFunctions.cs`** — вынести `private static long GCD(long, long)` из 4 классов в `PolyMath`
2. **`GetTotalSteps` / `GetStepIndex` в BotHandler** — рассмотреть паттерн Strategy или метод `GetActiveStepCount` на самом `FunctionBase` (сейчас cast-based)
3. **`Tokenization.cs`** — пустой класс, либо удалить, либо перенести туда `Lexer`
4. **Токен TELEGRAM_BOT_TOKEN** — добавить валидацию формата при старте
5. **Логи** — рассмотреть `ILogger<T>` или хотя бы ротацию файлов
