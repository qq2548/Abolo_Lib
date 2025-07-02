using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AboloLib
{
    [CreateAssetMenu(menuName = "ArtUtils/UIFactory")]
    public class UIFactory : ScriptableObject
    {
        public List<UI_Base> UI_Bases;
    }
}
