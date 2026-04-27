using System;
using System.Collections;
using System.Collections.Generic;
using ArtUtils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;
using Unity.VisualScripting;
//using UnityEngine.UIElements;

namespace AboloLib
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class UIGuideMask : Image,ICanvasRaycastFilter
    {

        private RectTransform _rayCastRectransform;
        protected override void Awake()
        {
            base.Awake();
            var rect_ojb = transform.Find("rect");
            if(rect_ojb != null)
            {
                _rayCastRectransform = transform.Find("rect").GetComponent<RectTransform>();
            }else
            {
                rect_ojb = new GameObject("rect").transform;
                _rayCastRectransform = rect_ojb.AddComponent<RectTransform>();
            }

            RefreshRayCastRectransform();
        }
#region Image重写

        static void AddQuad(VertexHelper vertexHelper , Vector2 posMin , Vector2 posMax,Color32 color , Vector2 uvMin , Vector2 uvMax)
        {
            int currentVertCount = vertexHelper.currentVertCount;
            vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0f), color, new Vector2(uvMin.x, uvMin.y));
            vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0f), color, new Vector2(uvMin.x, uvMax.y));
            vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0f), color, new Vector2(uvMax.x, uvMax.y));
            vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0f), color, new Vector2(uvMax.x, uvMin.y));
            vertexHelper.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
            vertexHelper.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount);
        }

        Vector4 GetAdjustedBorders(Vector4 border , Rect rect)
        {
            for (int axis = 0; axis <= 1; axis++)
            {
                float combinedBorders=border[axis]+border[axis + 2];
                if (rect.size[axis]<combinedBorders && combinedBorders!= 0)
                {
                    float borderScaleRatio=rect.size[axis]/combinedBorders;
                    border[axis]*=borderScaleRatio;
                    border[axis + 2]*=borderScaleRatio;
                }
            }

            return border;
        }


        private UIVertex[] _quad = new UIVertex[4];

        public Rect rect;
        protected override void OnPopulateMesh(VertexHelper vh)
        { 
            Vector4 outer , inner , padding , border;
            outer=DataUtility.GetOuterUV(sprite);
            inner=DataUtility.GetInnerUV(sprite);
            padding=DataUtility.GetPadding(sprite);
            border = sprite.border;
            padding = padding / multipliedPixelsPerUnit;
            //rect  = GetPixelAdjustedRect() ;
            border = GetAdjustedBorders(border/multipliedPixelsPerUnit , rect);
            float[] x = {0,0,0,0};
            x[0] = 0;
            x[1] = border.x;
            x[2] = rect.width - border.z;
            x[3] = rect.width;

            float[] y = {0,0,0,0};//{0+rect.y , rect.height + rect.y};
            y[0] = 0;
            y[1] = border.y;
            y[2] = rect.height - border.w;
            y[3] = rect.height;
            for (int i = 0; i < 4; i++)
            {
                x[i] += rect.x;
                y[i] += rect.y;
            }

            float[] x_uv = {0,0,0,0};
            x_uv[0] = 0;
            x_uv[1] = inner.x;
            x_uv[2] = inner.z;
            x_uv[3] = outer.z;

            float[] y_uv = {0,0,0,0};
            y_uv[0] = 0;
            y_uv[1] = inner.y;
            y_uv[2] = inner.w;
            y_uv[3] = outer.w;

            vh.Clear();

            for (int j = 0; j < 3; j++)
            {
                int num = j+1;
                for (int k = 0; k < 3; k++)
                {
                    int num2 = k+1;
                    AddQuad(vh, new Vector2(x[j], y[k]), new Vector2(x[num], y[num2]), 
                            color, 
                            new Vector2(x_uv[j], y_uv[k]), new Vector2(x_uv[num], y_uv[num2]));
                }
            }       
        }

        private void RefreshRayCastRectransform()
        {
            Vector3 pos = new Vector3(rect.position.x , rect.position.y , 0.0f);
            // pos = GameCameraAdapter.CurrentCamera.WorldToScreenPoint(pos);
            //pos = UICanvasAdapter.CurrentCanvas.worldCamera.ScreenToWorldPoint(rect.position);
            _rayCastRectransform.localPosition = rect.position + rect.size * 0.5f;
            _rayCastRectransform.sizeDelta = rect.size;
        }

        public void RectAnimation(Rect targetRect , float duration = 0.5f)
        {
            Vector2 fromPos = rect.position;
            Vector2 fromSize = rect.size;
            Action<float> _delta = (value) =>
            {
                Vector2 pos = Vector2.Lerp(fromPos, targetRect.position,value);
                Vector2 size = Vector2.Lerp(fromSize, targetRect.size, value);
                rect.Set(pos.x , pos.y , size.x , size.y) ;
                RefreshRayCastRectransform();
                SetVerticesDirty();
            };
            StartCoroutine(ArtAnimation.DoAnimation(duration , _delta));
        }

        public void testAnimation()
        {
            Action<float> _delta = (value) =>
            {
                Vector2 pos = Vector2.Lerp(new Vector2(0 , 0), new Vector2(-100 , 0),value);
                Vector2 size = Vector2.Lerp(new Vector2(100 , 200), new Vector2(200 , 100), value);
                rect.Set(pos.x , pos.y , size.x , size.y) ;
                RefreshRayCastRectransform();
                SetVerticesDirty();
            };
            StartCoroutine(ArtAnimation.DoAnimation(0.5f, _delta));
        }

#endregion


        /// <summary>
        /// target镂空区域不阻挡射线检测
        /// </summary>
        bool ICanvasRaycastFilter.IsRaycastLocationValid(Vector2 screenPos, Camera eventCamera)
        {
            //return false;
            return !RectTransformUtility.RectangleContainsScreenPoint(_rayCastRectransform, screenPos, eventCamera);
        }
    }
}
