using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AboloLib
{
    [CreateAssetMenu(menuName = "ArtUtils/PrefabFactory")]
    public class PrefabFactory : ScriptableObject
    {
        public List<GameObjectPreSet> MyPrefabs;
    }
    [System.Serializable]
    public struct GameObjectPreSet
    {
        public string name;
        public string discription;
        public GameObject prefab;
    }
}