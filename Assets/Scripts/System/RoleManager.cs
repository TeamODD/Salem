using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Round
{
    public int RoundNumber;
    public List<Role.Roles> RoleList;
}
public class RoleManager : MonoBehaviour
{
    public List<Character> Players = new List<Character>();
    public List<Round> Rounds = new List<Round>();

    private int _currentRoundIndex = -1;

    public void StartNextRound()
    {
        _currentRoundIndex++;
        if (_currentRoundIndex >= Rounds.Count)
        {
            return;
        }

        AssignRoles(_currentRoundIndex);
    }

    public void AssignRoles(int roundIndex)
    {
        foreach (var player in Players)
        {
            player.Deactive();
        }

        Round currentRound = Rounds[roundIndex];
        List<Role.Roles> roleDeck = new List<Role.Roles>(currentRound.RoleList);

        for (int i = 0; i < roleDeck.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, roleDeck.Count);
            Role.Roles temp = roleDeck[i];
            roleDeck[i] = roleDeck[randomIndex];
            roleDeck[randomIndex] = temp;
        }
        
        for (int i = 0; i < roleDeck.Count; i++)
        {
            if (i < Players.Count)
                Players[i].SetUpRole(roleDeck[i]);
        }
    }
}
