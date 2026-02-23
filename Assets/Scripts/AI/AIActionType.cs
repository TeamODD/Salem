public enum AIActionType
{
    None = 0,
    WitchAttack,
    BelieverInvestigate,
    BelieverStayHome,
    BelieverBodyFound,
    BelieverAbsent,
    BelieverRefused,
    ThiefLie,
    ThiefTruth,
    InsomniacWalk,
    InsomniacHome,
    CowardPlea,
    MuteSilent,
    CitizenHome
}

public static class AIActionTypeExtensions
{
    public static string ToActionIdString(this AIActionType actionType)
    {
        return actionType switch
        {
            AIActionType.WitchAttack => "witch_attack",
            AIActionType.BelieverInvestigate => "believer_investigate",
            AIActionType.BelieverStayHome => "believer_stay_home",
            AIActionType.BelieverBodyFound => "believer_body_found",
            AIActionType.BelieverAbsent => "believer_absent",
            AIActionType.BelieverRefused => "believer_refused",
            AIActionType.ThiefLie => "thief_lie",
            AIActionType.ThiefTruth => "thief_truth",
            AIActionType.InsomniacWalk => "insomniac_walk",
            AIActionType.InsomniacHome => "insomniac_home",
            AIActionType.CowardPlea => "coward_plea",
            AIActionType.MuteSilent => "mute_silent",
            AIActionType.CitizenHome => "citizen_home",
            _ => "none"
        };
    }

    public static bool TryParseActionId(string actionId, out AIActionType actionType)
    {
        switch (actionId)
        {
            case "witch_attack":
                actionType = AIActionType.WitchAttack;
                return true;
            case "believer_investigate":
                actionType = AIActionType.BelieverInvestigate;
                return true;
            case "believer_stay_home":
                actionType = AIActionType.BelieverStayHome;
                return true;
            case "believer_body_found":
                actionType = AIActionType.BelieverBodyFound;
                return true;
            case "believer_absent":
                actionType = AIActionType.BelieverAbsent;
                return true;
            case "believer_refused":
                actionType = AIActionType.BelieverRefused;
                return true;
            case "thief_lie":
                actionType = AIActionType.ThiefLie;
                return true;
            case "thief_truth":
                actionType = AIActionType.ThiefTruth;
                return true;
            case "insomniac_walk":
                actionType = AIActionType.InsomniacWalk;
                return true;
            case "insomniac_home":
                actionType = AIActionType.InsomniacHome;
                return true;
            case "coward_plea":
                actionType = AIActionType.CowardPlea;
                return true;
            case "mute_silent":
                actionType = AIActionType.MuteSilent;
                return true;
            case "citizen_home":
                actionType = AIActionType.CitizenHome;
                return true;
            default:
                actionType = AIActionType.None;
                return false;
        }
    }

    public static bool IsBelieverRelated(this AIActionType actionType)
    {
        return actionType == AIActionType.BelieverInvestigate ||
               actionType == AIActionType.BelieverStayHome ||
               actionType == AIActionType.BelieverBodyFound ||
               actionType == AIActionType.BelieverAbsent ||
               actionType == AIActionType.BelieverRefused;
    }
}
