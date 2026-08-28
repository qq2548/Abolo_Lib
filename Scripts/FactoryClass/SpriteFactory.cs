using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AboloLib
{
    [CreateAssetMenu(menuName = "ArtUtils/SpriteFactory")]
    public class SpriteFactory : ScriptableObject
    {
        public List<SpritePreset> spritePresets;
        public Sprite GetSprite(string name)
        {
            return spritePresets.FirstOrDefault(x => x.name == name).sprite;
        }
    }
    [System.Serializable]
    public class SpritePreset
    {
        public string name;
        public Sprite sprite;
    }
}
