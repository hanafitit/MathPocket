namespace MathPocket
{
    /// <summary>Теоретический материал (справка), доступный пользователю.</summary>
    public sealed class Material
    {
        public string   Name     { get; init; } = string.Empty;
        public string[] Keywords { get; init; } = [];
        public string   Content  { get; init; } = string.Empty;
    }
}
