using UnityEngine;

public class CitizenAI : CharacterAI
{
    private bool receivedPrayer = false;

    public override void DoNightAction(AIContext context)
    {
        // 시민은 밤에 특별한 행동을 하지 않고 집에 머무름
        Debug.Log($"[Citizen] {DisplayName} -> 집에 머무름 (시민)");
        SetAction(context, AIActionType.CitizenHome);
    }

    public override void ResolveMorning(AIContext context)
    {
        // 아침에 신자가 다녀갔는지 확인
        receivedPrayer = context.HasReceivedPrayer(this);
    }

    // 시민은 신자가 다녀갔는지 여부에 따라 대사가 달라짐
    public bool HasReceivedPrayer => receivedPrayer;

    public override bool TryGetReceivedPrayerForCitizenDialogue(out bool prayerReceived)
    {
        prayerReceived = receivedPrayer;
        return true;
    }
}
