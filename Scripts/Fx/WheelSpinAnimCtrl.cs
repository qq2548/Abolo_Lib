using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AboloLib
{
    public class WheelSpinAnimCtrl : MonoBehaviour
    {
        [SerializeField] float pointerAngle = 40f;
        [SerializeField] Animation _pointer;
        [SerializeField] Animation _light_ring;
        [SerializeField] Transform rot_root;
        [SerializeField] Transform rot_root_cover;
        [SerializeField] Transform lig_root;
        Transform rewarditem_root;
        [SerializeField] int totalResults = 15;
        [SerializeField] int cheatResult = 3;
        [SerializeField] int spinRound = 5;
        [SerializeField] float totalDuration = 5f;
        [SerializeField] AnimationCurve curve0;
        [SerializeField] AnimationCurve curve1;
        [SerializeField] AnimationCurve curve2;
        [SerializeField] Image ring_light;
        [SerializeField] Image blur_image;
        float unit_degrees = 20f;
        bool _initialized = false;
        Image[] _lights;
        Coroutine[] blinks_coroutines;

        float _animationDuration;
        public float AnimationDuration
        {
            get
            {
                return _animationDuration;
            }
            set
            {
                _animationDuration = value;
            }
        }
        private void Start()
        {
            Init();
        }

        static float[] blink_intervals = { 0.5f, 0.3f, 0.2f, 0.15f, 0.1f, 0.3f };
        public void Init()
        {
            if (!_initialized)
            {
#if _ARTEST_PRESENTATION
                var spin = transform.Find("root/SpinBtn");
                if (spin != null) spin.GetComponent<Button>().onClick.AddListener(() => TestSpin());
#endif
                SetAniamtionDuraion();
                ResetImage();
                rewarditem_root = transform.Find("root/wheel/root");
                if (lig_root != null)
                {
                    _lights = lig_root.GetComponentsInChildren<Image>(true);
                    blinks_coroutines = new Coroutine[_lights.Length];
                }
                unit_degrees = 360f / totalResults;
                _initialized = true;
            }
        }

        void SetAniamtionDuraion()
        {
            float blink_duration = 0f;
            for (int i = 0; i < blink_intervals.Length; i++)
            {
                blink_duration += blink_intervals[i];
            }
            _animationDuration = totalDuration + blink_duration;
        }

        private void ResetImage()
        {
            if (_light_ring != null)
            {
                _light_ring.GetComponent<CanvasGroup>().alpha = 0f;
            }
            if (_lights != null && _lights.Length > 0)
            {
                for (int i = 0; i < _lights.Length; i++)
                {
                    Color oriColor = _lights[i].color;
                    oriColor.a = 0f;
                    _lights[i].color = oriColor;
                }
            }
            if (ring_light != null)
            {
                Color oriColor = ring_light.color;
                oriColor.a = 0f;
                ring_light.color = oriColor;
            }

            if (blur_image != null)
            {
                Color oriColor = blur_image.color;
                oriColor.a = 0f;
                blur_image.color = oriColor;
            }
        }

        private void OnEnable()
        {
            if (!_initialized)
            {
                Init();
            }
            StartCoroutine(LightsIdleBreath());
        }

        private void OnDisable()
        {
            ResetImage();
            StopAllCoroutines();
        }

        public void Spin(int index , Action callback = null)
        {
            StopAllCoroutines();
            unit_degrees = 360f / totalResults;
            cheatResult = index;
            StartCoroutine(SpinAnimation(cheatResult , callback));
        }

        public void TestSpin()
        {
            StopAllCoroutines();
            unit_degrees = 360f / totalResults;
            StartCoroutine(SpinAnimation(cheatResult));
        }

        void LigtsAnimation(float t)
        {
            if (_lights != null && _lights.Length > 0)
            {
                for (int i = 0; i < _lights.Length; i++)
                {
                    Color fromColor = _lights[i].color;
                    float alpha = curve2.Evaluate(t);
                    _lights[i].color = new Color(fromColor.r, fromColor.g, fromColor.b, alpha);
                }
            }
        }

        void LigtsAnimation(float t , List<Image> imgs)
        {
            if (imgs != null && imgs.Count > 0)
            {
                for (int i = 0; i < imgs.Count; i++)
                {
                    Color fromColor = imgs[i].color;
                    float alpha = curve2.Evaluate(t);
                    imgs[i].color = new Color(fromColor.r, fromColor.g, fromColor.b, alpha);
                }
            }
        }

        void LigtsAnimation(float t, Image img)
        {
            if (img != null )
            {
                Color fromColor = img.color;
                float alpha = curve2.Evaluate(t);
                img.color = new Color(fromColor.r, fromColor.g, fromColor.b, alpha);
            }
        }

        IEnumerator  SpinAnimation(int result , Action callback = null)
        {
            if (_light_ring != null)
            {
                _light_ring.Play("ani_light_open");
            }
            float t = 0;
            Quaternion quaternion = rot_root.rotation;

            Vector3 from = new Vector3(0f , 0f , quaternion.eulerAngles.z%360f);
            float euler = spinRound * 360f + (unit_degrees * (totalResults - result) + quaternion.eulerAngles.z);
            StartCoroutine(LightsSpin());
            while (t <= 1.0f)
            {
                float pt = (euler * curve0.Evaluate(t) ) % unit_degrees;
                PointerAnimation(pt/ unit_degrees);
                float lpt = (euler * curve0.Evaluate(t)) % (72f);
                //LigtsAnimation(pt / unit_degrees);
                LigtsAnimation(pt / unit_degrees , ring_light);

                Color blur_color = blur_image.color;
                blur_color.a = Mathf.Lerp(0f , 1f , curve2.Evaluate(curve0.Evaluate(t)));
                blur_image.color = blur_color;

                //Debug.Log(t + "----------"+ curve0.Evaluate(t));
                quaternion.eulerAngles = from - new Vector3(0f, 0f, euler * curve0.Evaluate(t));
                rot_root.rotation = quaternion;
                if(rot_root_cover != null) rot_root_cover.rotation = quaternion;
                t += Time.deltaTime / totalDuration;

                yield return null;
            }
            StopCoroutine(LightsSpin());
            if (blinks_coroutines != null && blinks_coroutines.Length > 0)
            {
                for (int i = 0; i < blinks_coroutines.Length; i++)
                {
                    StopCoroutine(blinks_coroutines[i]);
                }
            }
            //这里收尾可以加一段拨片回弹的动画让效果更平滑。否则协程跑完拨片也会瞬间停止
            _pointer.Play();
            if (_light_ring != null)
            {
                _light_ring.Play("ani_light_close");
            }

            callback?.Invoke();

            yield return StartCoroutine(EndingLightsBlink());
            yield return new WaitForSeconds(1f);
            StartCoroutine(LightsIdleBreath());

#if _ARTEST_PRESENTATION
            //CurrencyCollectManager.CollectSpawmer.BoombToCollectCurrency(13, 1, transform, transform.position);
#endif
        }

        IEnumerator EndingLightsBlink()
        {
            for (int i = 0; i < blink_intervals.Length; i++)
            {
                float t = 0;
                while (t <= 1.0f)
                {
                    t += Time.deltaTime/ blink_intervals[i];
                    LigtsAnimation(t);
                    //LigtsAnimation(t , ring_light);
                    if (rewarditem_root != null)
                    {
                        LigtsAnimation(t, rewarditem_root.GetChild(cheatResult).Find("point").GetComponent<Image>());
                    }
                    yield return null;
                }
            }
        }


        IEnumerator LightsIdleBreath_Interval(List<Image> imgs)
        {
            while (this.gameObject.activeInHierarchy)
            {
                float t = 0f;
                while (t <= 1.0f)
                {
                    t += Time.deltaTime * 0.25f;
                    LigtsAnimation(t , imgs);
                    yield return null;
                }
            }
        }

        IEnumerator LightsBreath(Image img)
        {
            while (this.gameObject.activeInHierarchy)
            {
                float t = 0f;
                while (t <= 1.5f)
                {
                    t += Time.deltaTime * 2f;
                    LigtsAnimation(t, img);
                    yield return null;
                }
            }
        }

        IEnumerator LightsIdleBreath()
        {
            if (_lights != null && _lights.Length > 0)
            {
                List<Image>[] imgs_list = { new List<Image>(), new List<Image>(), new List<Image>() };
                for (int i = 0; i < _lights.Length; i++)
                {
                    int index = i % 3;
                    imgs_list[index].Add(_lights[i]);
                }
                for (int i = 0; i < 3; i++)
                {
                    StartCoroutine(LightsIdleBreath_Interval(imgs_list[i]));
                    yield return new WaitForSeconds(1f);
                }
            }
        }

        IEnumerator LightsSpin()
        {
            if (_lights != null && _lights.Length > 0)
            {
                for (int i = 0; i < _lights.Length; i++)
                {
                    blinks_coroutines[i] = StartCoroutine(LightsBreath(_lights[i]));
                    yield return new WaitForSeconds(0.05f);
                }
            }
        }


        void PointerAnimation(float t)
        {
            Vector3 from = new Vector3(0f,0f, pointerAngle);
            Vector3 to = Vector3.zero;
            Quaternion quaternion = Quaternion.identity;
            quaternion.eulerAngles = Vector3.LerpUnclamped(from , to , curve1.Evaluate(t));

            _pointer.transform.rotation = quaternion;
        }
    }
}
