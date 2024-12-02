using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AboloLib
{
    [RequireComponent(typeof(TMP_Text))]
    public class TMP_ColorModify : MonoBehaviour
    {
        [SerializeField] bool _changeOutlineColor;
        [SerializeField] Color _outlineColor = Color.white;

        [SerializeField] bool _changeOverLayColor;
        [SerializeField] Color _overlayColor = Color.white;
         
        [SerializeField] TMP_Text _tmpText ;
        Material _material;

        public bool ChangeOutlineColor { get => _changeOutlineColor; set => _changeOutlineColor = value; }
        public Color OutlineColor { get => _outlineColor; set => _outlineColor = value; }
        public bool ChangeOverLayColor { get => _changeOverLayColor; set => _changeOverLayColor = value; }
        public Color OverlayColor { get => _overlayColor; set => _overlayColor = value; }

        IEnumerator  Start()
        {
            yield return null;
            Modify();
        }

        bool _settled = false;
        private void OnEnable()
        {
            if (!_settled)
            {
                Modify();
            }
        }
        //        protected   void OnValidate()
        //        {
        //#if UNITY_EDITOR
        //            Modify();
        //            UnityEditor.EditorUtility.SetDirty(transform.gameObject);
        //#endif
        //        }

        public void Modify()
        {
            if (_tmpText == null) _tmpText = GetComponent<TextMeshProUGUI>();
            if (ChangeOutlineColor)
            {
                SetOutlineColor();
            }
            if (ChangeOverLayColor)
            {
                SetOverlayColor();
            }
            _settled = true;
        }

        void SetOutlineColor()
        {
            if (_tmpText == null) _tmpText = GetComponent<TextMeshProUGUI>();
            Material originalMaterial = _tmpText.materialForRendering;
            Material newMaterial = new Material(originalMaterial);
            newMaterial.SetColor("_OutlineColor", OutlineColor);
            if (!newMaterial.IsKeywordEnabled("OUTLINE_ON"))
            {
                newMaterial.EnableKeyword("OUTLINE_ON");
            }
            _tmpText.fontMaterial = newMaterial;
        }

        void SetOverlayColor()
        {
            if (_tmpText == null) _tmpText = GetComponent<TextMeshProUGUI>();
            Material originalMaterial = _tmpText.materialForRendering;
            Material newMaterial = new Material(originalMaterial);
            if (!newMaterial.IsKeywordEnabled("UNDERLAY_ON"))
            {
                newMaterial.EnableKeyword("UNDERLAY_ON");
            }
            newMaterial.SetColor("_UnderlayColor", OverlayColor);
            _tmpText.fontMaterial = newMaterial;
        }
    }
}