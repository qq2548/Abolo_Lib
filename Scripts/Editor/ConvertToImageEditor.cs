using UnityEngine;
using UnityEditor;
using ArtUtils;

namespace AboloLib
{

    [CustomEditor(typeof(ConvertToImage))]
    public class ConvertToImageEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            ConvertToImage configName = (ConvertToImage)target;
            if (GUILayout.Button("Sprite转换Image"))
            {
                configName.ConvertSpriteRenererToImage();
            }
        }
    }
}
