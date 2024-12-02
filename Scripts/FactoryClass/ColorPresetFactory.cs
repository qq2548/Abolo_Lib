using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AboloLib
{
    [CreateAssetMenu(menuName = "ArtUtils/ColorPreset")]
    public class ColorPresetFactory : ScriptableObject
    {
        public  List<ColorPreset> ColorPresets;


        public  Color GetCharaterColor( string _name ) {
            foreach( var pair in ColorPresets ) {
                if( pair.name == _name ) {
                    return pair.myColor;
                }
            }
            return Color.white;
        }
    }
    [System.Serializable]
    public class ColorPreset
    {
        public string name;
        public Color myColor = Color.white;
    }
}
