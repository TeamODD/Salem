using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public const int RoundBonusPerRound = 500;
    public const int SacrificePenaltyPerPerson = 100;
    public const int CorrectMemoBonus = 100;

    public struct ScoreResult
    {
        public int RoundsCompleted;
        public int SacrificedCountExcludingWitch;
        public int CorrectMemoCount;
        public int TotalScore;
        public string Grade;
    }

    public static ScoreResult CalculateScore(
        int roundsCompleted,
        int sacrificedCountExcludingWitch,
        int correctMemoCount)
    {
        int safeRounds = Mathf.Max(0, roundsCompleted);
        int safeSacrificed = Mathf.Max(0, sacrificedCountExcludingWitch);
        int safeCorrectMemos = Mathf.Max(0, correctMemoCount);

        int totalScore =
            (safeRounds * RoundBonusPerRound) -
            (safeSacrificed * SacrificePenaltyPerPerson) +
            (safeCorrectMemos * CorrectMemoBonus);

        return new ScoreResult
        {
            RoundsCompleted = safeRounds,
            SacrificedCountExcludingWitch = safeSacrificed,
            CorrectMemoCount = safeCorrectMemos,
            TotalScore = totalScore,
            Grade = GetGrade(totalScore)
        };
    }

    public static string GetGrade(int totalScore)
    {
        if (totalScore >= 5001) return "S";
        if (totalScore >= 4001) return "A";
        if (totalScore >= 3001) return "B";
        if (totalScore >= 2001) return "C";
        if (totalScore >= 1001) return "D";
        return "F";
    }
}
