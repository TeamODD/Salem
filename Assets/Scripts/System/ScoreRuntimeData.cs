public static class ScoreRuntimeData
{
    public static bool HasData { get; private set; }
    public static bool IsVictory { get; private set; }
    public static ScoreManager.ScoreResult LastResult { get; private set; }

    public static void Set(ScoreManager.ScoreResult result, bool isVictory)
    {
        LastResult = result;
        IsVictory = isVictory;
        HasData = true;
    }

    public static void Clear()
    {
        HasData = false;
        IsVictory = false;
        LastResult = default;
    }
}
