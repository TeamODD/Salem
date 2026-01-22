using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/CharacterData")]
public class CharacterData : ScriptableObject
{
    [Header("Basic Info")]
    public string characterName;
    public Sprite defaultSprite;

    [Header("Dialogue Node")]
    public string dialogueNodeName;

    [Header("Scale Setting")]
    public float baseScale = 1.0f;
    public float hoverScaleFactor = 1.2f;
}
