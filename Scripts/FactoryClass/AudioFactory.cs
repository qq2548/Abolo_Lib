using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AboloLib
{
    [CreateAssetMenu(menuName = "ArtUtils/AudioFactory")]
    public class AudioFactory : ScriptableObject
    {
        public enum AudioName
        {
            hotspot_Hammer_Multiple,
            hotspot_Hammer_Single,
            hotspot_Clean,
            hotspot_Clean_Chairs,
            hotspot_Clean_Leaves_Once,
            hotspot_Clean_Leaves_Twice,
            hotspot_BrandNew,
            Newitem_Generate_Star,
            Merge_Normal,
            Merge_MaxLv,
            Merge_Pop,
            Merge_Drop,
            Merge_Open,
            Merge_Close,
            Merge_BGM_1,
            Battle_BGM_1,
#if _ARTEST_PRESENTATION
            BGM,
#endif
        }

        public List<AudioPreset> AudioPresets;
    }

    [System.Serializable]
    public class AudioPreset
    {
        public string _mName;
        public AudioClip  mAudioClip;
        public float  delay = 0.5f;
    }
}