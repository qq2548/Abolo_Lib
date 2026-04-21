using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using TMPro;
using System.Threading;

namespace AboloLib
{
    [ExecuteInEditMode]
    public class ConvertToImage : MonoBehaviour
    {
        public static void ConvertSpriteRenerersToImage(Transform target , float scalor = 100f)
        { 
            target.localScale = Vector3.one * scalor;
            List<Vector3> Positions = new List<Vector3>();
            Positions.Clear();
            var trans = target.GetComponentsInChildren<Transform>();
            //var trans = transtmp.Where(x => x != this.transform).ToArray();
            List<RectTransform> rectTransforms = new List<RectTransform>();
            rectTransforms.Clear();
            foreach (var item in trans)
            {
                Positions.Add(item.position);
                //target这里好像会被异步销毁，需要另外赋值
                var rectTransform = item.gameObject.AddComponent<RectTransform>();
                rectTransforms.Add(rectTransform);
            }
            //target 重新赋值 
            target = rectTransforms[0].transform;
            var sp = target.GetComponentsInChildren<SpriteRenderer>(true);
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

            target.localScale = Vector3.one;

            var trans1 = target.GetComponentsInChildren<Transform>();
            for (int i = 0; i < trans1.Length; i++)
            {
                trans1[i].position = Positions[i];
                trans1[i].localScale = Vector3.one;
                trans1[i].TryGetComponent(out Image image);
                if (image != null)
                {
                    Vector2 size = trans1[i].GetComponent<RectTransform>().sizeDelta;
                    trans1[i].GetComponent<RectTransform>().sizeDelta = new Vector2(size.x * scalor , size.y * scalor);
                }
            }
            ReverseChildrenHierarchy(target);
            SetChildrenComponents(target);
        }
        [SerializeField] float Scalor = 100f;
       public void ConvertSpriteRenererToImage()
        {
            transform.localScale = Vector3.one * Scalor;

            List<Vector3> Positions = new List<Vector3>();
            Positions.Clear();
            var trans = GetComponentsInChildren<Transform>();
            //var trans = transtmp.Where(x => x != this.transform).ToArray();
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
            ReverseChildrenHierarchy(this.transform);
            SetChildrenComponents(this.transform);
        }

        public static void ReverseChildrenHierarchy(Transform parent)
        {
            if (parent.childCount >0)
            {
                var cc = parent.GetComponentsInChildren<Transform>();
                for (int i = 0; i < cc.Length; i++)
                {
                    if(cc[i] != parent) cc[i].SetAsFirstSibling();
                }
            }
        }

        static void SetChildrenComponents(Transform parent)
        {
            if (parent.childCount >0)
            {
                var cc = parent.GetComponentsInChildren<Transform>();
                for (int i = 0; i < cc.Length; i++)
                {
                    SetUIComponents(cc[i]);
                }
            }
        }
        //修复父节点位置在零点的问题
        static void FixParentNodePosition(Transform node)
        {
            var childrentmp = node.GetComponentsInChildren<Transform>();
            var children = childrentmp.Where(x => x != node).ToArray();
            var posmark = node.GetComponentInChildren<Graphic>();
            foreach (Transform child in children)
            {
                child.SetParent(node.parent);
            }
            if (posmark != null)
            {
                node.position = posmark.transform.position;
            }
            else
            {
                Debug.LogWarning($"Missing node: {"Bg"},Check your file!!");
            }  
            foreach (Transform child in children)
            {
                child.SetParent(node);
            }            
        }

        static void SetUIComponents(Transform node)
        {
            string nodeName = node.gameObject.name;
        
            if(nodeName.Contains("Slider"))
            {
                FixParentNodePosition(node);
                var slider = node.AddComponent<Slider>();
                slider.transition = Selectable.Transition.None;
                var fill = node.Find("Fill");

                if (fill != null)
                {
                    slider.targetGraphic = fill.GetComponent<Graphic>();
                    var fillrect = fill.GetComponent<RectTransform>();
                    Vector2 size = fillrect.sizeDelta;
                    slider.GetComponent<RectTransform>().sizeDelta = size;
                    slider.fillRect = fillrect;
                    fillrect.anchorMin = Vector2.zero;
                    fillrect.anchorMax = Vector2.one;
                    fillrect.anchoredPosition = Vector2.zero;
                    fillrect.sizeDelta = Vector2.zero; 
                    var fill_image = fillrect.GetComponent<Image>();
                    if (fill_image != null) fill_image.type =  Image.Type.Sliced;
                }
                
                var handle = node.Find("Handle");

                if (handle != null)
                {
                    var handlerect = handle.GetComponent<RectTransform>();
                    var handlesize = handle.GetComponent<RectTransform>().sizeDelta;
                    slider.handleRect = handle.GetComponent<RectTransform>();
                    handlerect.anchoredPosition = Vector2.zero;
                    handlerect.sizeDelta = new Vector2(handlesize.x , handlesize.y * 0.5f);
                }
            }
            if(nodeName.Contains("Btn_"))
            {
                FixParentNodePosition(node);
                var btn = node.AddComponent<Button>();   
                btn.transition = Selectable.Transition.None;
                btn.targetGraphic = node.GetComponentInChildren<Graphic>();
            }
            if(nodeName.Contains("TmpText_"))
            {
                var img = node.GetComponent<Image>();
                if(img != null)
                {
                     GameObject.DestroyImmediate(img);
                }
                else
                {
                    node.transform.localPosition = Vector3.zero;
                }
                var tmp_text = node.AddComponent<TextMeshProUGUI>();
                var info = nodeName.Split("_");  
                node.name = info[0];
                tmp_text.text = info[1];
                float fontSize = float.Parse(info[2]);
                tmp_text.fontSize = fontSize;
                tmp_text.enableWordWrapping = false;
                tmp_text.alignment = TextAlignmentOptions.Center;
                tmp_text.characterSpacing = -7.25f;
                UnityEngine.ColorUtility.TryParseHtmlString(info[3] , out Color color);
                //color = new Color(Mathf.Pow(color.r , 2.2f) , Mathf.Pow(color.g , 2.2f),Mathf.Pow(color.b , 2.2f) ,1.0f);
                tmp_text.color = color; 
                var rectTransform = node.GetComponent<RectTransform>();
                var ancPos = rectTransform.anchoredPosition;
                rectTransform.anchoredPosition = new Vector2(ancPos.x , ancPos.y );
            }
        }
    }
} 