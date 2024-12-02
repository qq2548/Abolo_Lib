using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AboloLib
{
    [ExecuteInEditMode]
    public class ConvertToImage : MonoBehaviour
    {
        [SerializeField] float Scalor = 100f;
       public void ConvertSpriteRenererToImage()
        {
            transform.localScale = Vector3.one * Scalor;

            List<Vector3> Positions = new List<Vector3>();
            Positions.Clear();
            var trans = GetComponentsInChildren<Transform>();
            foreach (var item in trans)
            {
                Positions.Add(item.position);
                item.gameObject.AddComponent<RectTransform>();
            }


            var sp = GetComponentsInChildren<SpriteRenderer>(true);
            List<Image> images = new List<Image>();
            images.Clear();
            if (sp != null && sp.Length >0)
            {
                for (int i = 0; i < sp.Length; i++)
                {
                    //sp[i].gameObject.AddComponent<RectTransform>();
                    var img = sp[i].gameObject.AddComponent<Image>();
                    img.sprite = sp[i].sprite;
                    //img.transform.SetAsFirstSibling();
                    images.Add(img);
                }
            }
            if (images != null && images.Count >0)
            {
                foreach (var item in images)
                {
                    DestroyImmediate(item.transform.GetComponent<SpriteRenderer>());
                }
            }

            transform.localScale = Vector3.one;

            var trans1 = GetComponentsInChildren<Transform>();
            for (int i = 0; i < trans1.Length; i++)
            {
                trans1[i].position = Positions[i];
                trans1[i].localScale = Vector3.one;
                trans1[i].TryGetComponent(out Image image);
                if (image != null)
                {
                    Vector2 size = trans1[i].GetComponent<RectTransform>().sizeDelta;
                    trans1[i].GetComponent<RectTransform>().sizeDelta = new Vector2(size.x * Scalor , size.y * Scalor);
                }
            }
            ReverseChildrenHierarchy(transform);
        }

        public void ReverseChildrenHierarchy(Transform parent)
        {
            if (parent.childCount >0)
            {
                var cc = transform.GetComponentsInChildren<Transform>();
                for (int i = 0; i < cc.Length; i++)
                {
                    cc[i].SetAsFirstSibling();
                }
            }
        }
    }
}
