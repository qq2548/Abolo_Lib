using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AboloLib
{
    public class BgFromCamera : ArtAnimation
    {
        public static BgFromCamera instance;
        //正式版需要拿到主摄像机的引用
        Camera grabCamera;
        [SerializeField] RawImage rawImageBg;
        [SerializeField] Material mDrawMat;
        //动画时长
        [SerializeField] float duration;
        private RenderTexture renderTexture;
        private RenderTexture renderTexture2;
        void Awake()
        {
            instance = this;
        }

        public override void Start()
        {
            base.Start();
            grabCamera = GameCameraAdapter.CurrentCamera;
            //grabCamera.depthTextureMode |= DepthTextureMode.Depth;
        }


        //IEnumerator SetBgRenderTexture()
        //{
        //    //创建RT depth 参数不能为零，否则无法渲染出3D 几何层
        //    renderTexture = new RenderTexture((int)Screen.width >> 3, (int)Screen.height >> 3, 24, RenderTextureFormat.ARGBFloat);
        //    CameraBlur cb;
        //    if (!grabCamera.TryGetComponent(out cb))
        //    {
        //        cb = grabCamera.gameObject.AddComponent<CameraBlur>();
        //        cb.shader = Shader.Find("2d/BlurEffect_5X5");
        //        cb.blurRange = 4.1f;
        //    }
        //    else
        //    {
        //        cb.enabled = true;
        //    }
        //    Debug.Log(cb.name);
        //    yield return new WaitForEndOfFrame();

        //    grabCamera.targetTexture = renderTexture;
        //    grabCamera.Render();
        //    grabCamera.targetTexture = null;
        //    cb.enabled = false;
        //    rawImageBg.texture = renderTexture;
        //}

        IEnumerator GrabBackGround(Action callback)
        {
            //创建RT depth 参数不能为零，否则无法渲染出3D 几何层，不要设置RT的RenderTextureMode，华为手机要出渲染bug，很牛
            renderTexture = RenderTexture.GetTemporary(Screen.width >> 2, Screen.height >> 2, 24);
            yield return new WaitForEndOfFrame();
            grabCamera = GameCameraAdapter.CurrentCamera;
            grabCamera.targetTexture = renderTexture;
            grabCamera.Render();
            mDrawMat.SetTexture("_MainTex" , renderTexture);
            //mDrawMat.SetFloat("_Range" , 4.1f);
            renderTexture2 = RenderTexture.GetTemporary(renderTexture.width, renderTexture.height);
            Graphics.Blit(renderTexture, renderTexture2, mDrawMat);
            grabCamera.targetTexture = null;
            rawImageBg.texture = renderTexture2;
            yield return new WaitForEndOfFrame();
            callback?.Invoke();
        }

        public void PlayOpen()
        {
            StopAnimation();
            Color from = rawImageBg.color;
            Action<float> _deltaAnimation = ((value) =>
            {
                rawImageBg.color = ArtUtility.FadeIn(inputColor : from, value , CurveAdapter.AnimCurveDic[CurveFactory.CurveType.FlyPosition]);
            });
            Action callback = ()=> ani = StartCoroutine(DoAnimation(duration, _deltaAnimation));
            //Action callback = new Action( ()=> ani = StartCoroutine(GraphicFadeIn(rawImageBg, duration, AnimCurveDic[CurveFactory.CurveType.FlyPosition])));
            StartCoroutine(GrabBackGround(callback));
        }

        public void PlayClose()
        {
            StopAnimation();
            Color from = rawImageBg.color;
            Action<float> _deltaAnimation = ((value) =>
            {
                rawImageBg.color = ArtUtility.FadeOut(inputColor : from, value);
            }) ;
            //Action<float> action = (value) => { rawImageBg.color = ArtUtility.FadeOut(rawImageBg.color, value); };
            //ani = StartCoroutine(GraphicFadeOut(rawImageBg, duration, ()=>RealeaseBuffer()));
            ani = StartCoroutine(DoAnimation(duration, _deltaAnimation, () => RealeaseBuffer()));
        }

        void RealeaseBuffer()
        {
            if (renderTexture != null) renderTexture.Release();
            if (renderTexture2 != null) renderTexture2.Release();
        }
    }
}
