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
        public RawImage  RawImageBg
        {
            get { return rawImageBg; }
        }
        [SerializeField] Material mDrawMat;
        //动画时长
        [SerializeField] float duration;
        private RenderTexture renderTexture_game;
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
            renderTexture_game = RenderTexture.GetTemporary(Screen.width >> 2, Screen.height >> 2, 24);
            yield return new WaitForEndOfFrame();
            grabCamera = GameCameraAdapter.CurrentCamera;
            grabCamera.targetTexture = renderTexture_game;
            grabCamera.Render();
            mDrawMat.SetTexture("_MainTex" , renderTexture_game);
            mDrawMat.SetFloat("_Range" , 4.1f);
            renderTexture2 = RenderTexture.GetTemporary(renderTexture_game.width, renderTexture_game.height);
            Graphics.Blit(renderTexture_game, renderTexture2, mDrawMat);
            grabCamera.targetTexture = null;
            rawImageBg.texture = renderTexture2;
            yield return new WaitForEndOfFrame();
            callback?.Invoke();
        }

        IEnumerator GrabFullBackGround(RawImage rawImage ,Action callback)
        {
            //创建RT depth 参数不能为零，否则无法渲染出3D 几何层，不要设置RT的RenderTextureMode，华为手机要出渲染bug，很牛
            renderTexture_game = RenderTexture.GetTemporary(Screen.width >> 2, Screen.height >> 2, 24);
            var renderTexture_ui = RenderTexture.GetTemporary(Screen.width >> 2, Screen.height >> 2, 24);
            yield return new WaitForEndOfFrame();
            grabCamera = GameCameraAdapter.CurrentCamera;
            grabCamera.targetTexture = renderTexture_game;
            grabCamera.Render();

            RenderTexture.active = renderTexture_game;
            Texture2D texture2D = new Texture2D(renderTexture_game.width , renderTexture_game.height);
            texture2D.ReadPixels(new Rect(0 , 0 , renderTexture_game.width , renderTexture_game.height) , 0 , 0);
            texture2D.Apply();
            Color[] colors_1 = texture2D.GetPixels();

            Camera ui_grab = UICanvasAdapter.CurrentCanvas.worldCamera;
            ui_grab.targetTexture = renderTexture_ui;
            ui_grab.Render();

            RenderTexture.active = renderTexture_ui;
            Texture2D texture2D_ui = new Texture2D(renderTexture_ui.width , renderTexture_ui.height);
            texture2D_ui.ReadPixels(new Rect(0 , 0 , renderTexture_ui.width , renderTexture_ui.height) , 0 , 0);
            texture2D_ui.Apply();
            Texture2D result = CombineTwoTexture(texture2D_ui , texture2D) ;
            mDrawMat.SetTexture("_MainTex" , result);
            mDrawMat.SetFloat("_Range" , 4.1f);
            renderTexture2 = RenderTexture.GetTemporary(renderTexture_game.width, renderTexture_game.height);
            Graphics.Blit(result, renderTexture2, mDrawMat);
            grabCamera.targetTexture = null;
            ui_grab.targetTexture = null;
            renderTexture_ui.Release();
            
            rawImageBg.texture = renderTexture2;
            rawImage.texture = renderTexture2;
            yield return new WaitForEndOfFrame();
            callback?.Invoke();
        }

        public void PlayOpen()
        {
            StopAnimation();
            Color from = rawImageBg.color;
            Action<float> _deltaAnimation = (value) =>
            {
                rawImageBg.color = ArtUtility.FadeIn(inputColor : from, value , CurveAdapter.AnimCurveDic[CurveFactory.CurveType.FlyPosition]);
            };
            Action callback = ()=> ani = StartCoroutine(DoAnimation(duration, _deltaAnimation));

            StartCoroutine(GrabBackGround(callback));
        }

        public void PlayClose()
        {
            StopAnimation();
            Color from = rawImageBg.color;
            Action<float> _deltaAnimation = (value) =>
            {
                rawImageBg.color = ArtUtility.FadeOut(inputColor : from, value);
            };
            ani = StartCoroutine(DoAnimation(duration, _deltaAnimation, () => RealeaseBuffer()));
        }

        public void PlayFullOpen(RawImage rawImage)
        {
            StopAnimation();
            Color from = rawImage.color;
            Action<float> _deltaAnimation = ((value) =>
            {
                rawImage.color = ArtUtility.FadeIn(inputColor : from, value , CurveAdapter.AnimCurveDic[CurveFactory.CurveType.FlyPosition]);
            });
            Action callback = ()=> ani = StartCoroutine(DoAnimation(duration, _deltaAnimation));
            StartCoroutine(GrabFullBackGround(rawImage , callback));
        }

        public void PlayFullClose(RawImage rawImage)
        {
            StopAnimation();
            Color from = rawImage.color;
            Action<float> _deltaAnimation = (value) =>
            {
                rawImage.color = ArtUtility.FadeOut(inputColor : from, value);
            };
            ani = StartCoroutine(DoAnimation(duration, _deltaAnimation, () => RealeaseBuffer()));
        }

        public Texture2D CombineTwoTexture(Texture2D fstTex , Texture2D secTex)
        {
            Texture2D texture= new Texture2D(fstTex.width,fstTex.height);
            Color[] color_1 = fstTex.GetPixels();
            Color[] color_2 = secTex.GetPixels();
            Color[] color_3 = new Color[color_1.Length];
            for (int i = 0; i < color_1.Length; i++)
            {
                float r = Mix(color_1[i].r , color_2[i].r , color_1[i].a);
                float g = Mix(color_1[i].g , color_2[i].g , color_1[i].a);
                float b = Mix(color_1[i].b , color_2[i].b , color_1[i].a);
                color_3[i] = new Color( r, g , b , 1.0f);
            }
            texture.SetPixels(color_3);
            texture.Apply();
            return texture;
        }

        public float Mix(float a , float b , float t)
        {
            return a * t + b * (1.0f - t);
        }

        void RealeaseBuffer()
        {
            if (renderTexture_game != null) renderTexture_game.Release();
            if (renderTexture2 != null) renderTexture2.Release();
        }
    }
}
