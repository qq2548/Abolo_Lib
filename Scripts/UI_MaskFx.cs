using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AboloLib
{
    public class UI_MaskFx : ArtAnimation,IFxAnimCtrl,ICanvasRaycastFilter
    {
        private static string[,] _shaderPropNames = { { "_centerPosX", "_centerPosY" },
                                                                               { "_centerPosX2", "_centerPosY2" },
                                                                               { "_centerPosX3", "_centerPosY3" } };
        [Header("遮罩Image")]
        [SerializeField] Image _image;
        [SerializeField]int _shapeCount;
        [Header("播放完毕后镂空遮罩停留的大小")]
        [Range(0.0f, 1.0f)]
        [SerializeField] float _hollowRange;
        [Header("需要遮罩指示的目标")]
        [SerializeField] RectTransform[] targets;
        //使用Rect遮罩把材质的UseRectMask开关勾选，UseSpriteMask开关取消
        //使用Sprite遮罩把材质的UseSpriteMask开关勾选，UseRectMask开关取消
        //不兼容拉伸对齐的UI物体
        [Tooltip("使用Sprite遮罩把材质的UseSpriteMask开关勾选，UseRectMask开关取消")]
        //是否根据传入的RectTransform物体的Image贴图替换Sprite遮罩，这个效果要配合打开材质的Sprite遮罩开关
        [SerializeField] bool _useSpriteShape;
        //是否使用屏幕中心作为蒙版位置
        [SerializeField] bool _useScreenCenter;
        //public AnimationCurve animationCurve;
        [Header("遮罩边缘外扩权重")]
        public float RangeExpand = 0.1f;

        private Vector2 _rectSize;
        private Camera _camera;
        private float _canvasScale;
        private float _rectScale;
        private Canvas _canvas;

        Material _material;

        private void Awake()
        {
            _rectScale = 15f;

            //new 个实例不用每次修改都会改动到asset的material
            _material = new Material(_image.material);

            if (_material != null)
            {
                _image.material = _material;
            }
            _image.gameObject.SetActive(false);
            SetMaskRectSize(Vector2.zero);
        }

        public override void Start()
        {
            base.Start();
            _canvas = UICanvasAdapter.CurrentCanvas;
            SetMyCanmera();
#if _ARTEST_PRESENTATION
            Debug.Log(Screen.width + "---" + Screen.height);
#endif
        }

        void SetMyCanmera()
        {
            if (_useScreenCenter)
            {
                _camera = null;
            }
            else if (_canvas != null)
            {
                _camera = _canvas.worldCamera;
            }
        }
        public void SetCanvas(Canvas canvas)
        {
            if (_canvas == null)
            {
                _canvas = canvas;
                SetMyCanmera();
            }
        }

        void Configuration()
        {
            _canvasScale = _canvas.scaleFactor;
            //mask范围外扩
            float val = Mathf.Min(Screen.width, Screen.height) * RangeExpand;
            _rectSize = targets[0].sizeDelta + new Vector2(val, val) / _canvasScale;
            if (!_image.gameObject.activeSelf)
            {
                _image.gameObject.SetActive(true);
            }
        }
        public void ShowMask()
        {
            if( _canvas == null ) {
                _canvas = UICanvasAdapter.CurrentCanvas;
                SetMyCanmera();
            }
            SetMaskCenterPosition(targets);
            Configuration();
            SetMaskRectSize(_rectSize);
        }

        public void PlayMaskAnimation()
        {
            StopAnimation();

            ani = StartCoroutine(MaskAnimation(targets, CurveAdapter.CurveFactory.durationPreset9));
        }

        public void PlayReverseMaskAnimation()
        {
            StopAnimation();

            ani = StartCoroutine(MaskAnimation(targets, CurveAdapter.CurveFactory.durationPreset9 , true));
        }

        public void PlayMaskAnimation(RectTransform[] rectTransforms, float duration = 1f)
        {
            StopAnimation();
            ani = StartCoroutine(MaskAnimation(rectTransforms, duration));
        }

        public void SetMaskCenterPosition(RectTransform[] rectTransforms)
        {
            if (_useSpriteShape)
            {
                var tex = rectTransforms[0].GetComponent<Image>().sprite.texture;
                _image.material.SetTexture("_subTex", tex);
            }
            _image.material.SetFloat("_screenWidth", Screen.width);
            _image.material.SetFloat("_screenHeight", Screen.height);

            _image.material.SetFloat("_Count", rectTransforms.Length);

            for (int a = 0; a < rectTransforms.Length; a++)
            {
                Vector3 tPos = rectTransforms[a].position;
                if (_camera != null)
                {
                    tPos = RectTransformUtility.WorldToScreenPoint(_camera, tPos);
                }
                else
                {
                    tPos = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, -10f);
                }
                Vector2 centerPosToMat = new Vector2((tPos.x + rectTransforms[a].sizeDelta.x * (0.5f - rectTransforms[a].pivot.x) * _canvasScale) / Screen.width,
                                                                                 (tPos.y + rectTransforms[a].sizeDelta.y * (0.5f - rectTransforms[a].pivot.y) * _canvasScale) / Screen.height);


                _image.material.SetFloat(_shaderPropNames[a, 0], centerPosToMat.x);
                _image.material.SetFloat(_shaderPropNames[a, 1], centerPosToMat.y);
            }
        }

        private void SetMaskRectSize(Vector2 rectSize)
        {

            float rectX = rectSize.x / Screen.width * _canvasScale;
            float rectY = rectSize.y / Screen.height * _canvasScale;
            _image.material.SetFloat("_rectWidth", rectX);
            _image.material.SetFloat("_rectHeight", rectY);
        }

        IEnumerator MaskAnimation(RectTransform[] rectTransforms, float duration = 0.5f,bool _reverse = false)
        {
            SetMaskCenterPosition(targets);
            Configuration();
            ///暂时写死0.75s 动画周期
            float _timer = duration;
            if (!_reverse)
            {
                while (_timer > 0.0f)
                {
                    _timer -= Time.deltaTime;
                    float t = 1.0f - _timer / duration;
                    t = 1.0f - CurveAdapter.AnimCurveDic[CurveFactory.CurveType.SlowSteady].Evaluate(t);
                    Vector2 rectSize = _rectSize * Mathf.Clamp(t * _rectScale + _hollowRange, _hollowRange, 66f);
                    SetMaskRectSize(rectSize);

                    yield return null;
                }
            }
            else
            {
                while (_timer > 0.0f)
                {
                    _timer -= Time.deltaTime;
                    float t =  _timer / duration;
                    t = ArtUtility.DecreaseLinearCurve.Evaluate(t);
                    Vector2 rectSize = _rectSize * Mathf.Clamp(t * _rectScale + _hollowRange, _hollowRange, 66f);
                    SetMaskRectSize(rectSize);

                    yield return null;
                }
            }

        }

        public void Play()
        {
            if (TryGetComponent(out Animation animation))
            {
                animation.Play(StartAnimeName);
            }
           
        }

        public  void Stop(Action callback = null)
        {
            if (TryGetComponent(out Animation animation))
            {
                animation.Play(StopAnimeName);
                if (callback != null)
                {
                    StartCoroutine(ArtAnimDelayCoroutine(CurveAdapter.CurveFactory.durationPreset9, callback));
                }
            }
        }

        /// <summary>
        /// target镂空区域不阻挡射线检测
        /// </summary>
        bool ICanvasRaycastFilter.IsRaycastLocationValid(Vector2 screenPos, Camera eventCamera)
        {
            if (targets[0] != null)
            {
                return !RectTransformUtility.RectangleContainsScreenPoint(targets[0], screenPos, eventCamera); ;
            }
            else
            {
                return true;
            }
        }
    }

}