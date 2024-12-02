using UnityEngine;

namespace AboloLib
{
    public class GrowAnimationTypeInfo : MonoBehaviour
    {
        [SerializeField]
         GrowAnimationType animationType = GrowAnimationType.Pop;
        public GrowAnimationType AnimType
        {
            get => animationType;
            set => animationType = value;
        }
    }
}
