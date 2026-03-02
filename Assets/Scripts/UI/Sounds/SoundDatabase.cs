using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SoundDatabase", menuName = "Salem/SoundDatabase")]
public class SoundDatabase : ScriptableObject
{
    [System.Serializable]
    public class BGMMapping
    {
        public BGMType Type;
        public AudioClip Clip;
    }

    [System.Serializable]
    public class SFXMapping
    {
        public SFXType Type;
        public AudioClip Clip;
    }

    [Header("BGM 설정")]
    public List<BGMMapping> BGMList = new List<BGMMapping>();

    [Header("효과음 설정")]
    public List<SFXMapping> SFXList = new List<SFXMapping>();

    public AudioClip GetBGMClip(BGMType type)
    {
        var mapping = BGMList.Find(x => x.Type == type);
        return mapping != null ? mapping.Clip : null;
    }

    public AudioClip GetSFXClip(SFXType type)
    {
        var mapping = SFXList.Find(x => x.Type == type);
        return mapping != null ? mapping.Clip : null;
    }
}
