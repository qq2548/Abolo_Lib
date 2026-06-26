
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using System.Threading;
using Unity.VisualScripting;
using System.Linq;

namespace AboloLib
{
    public class UI_CustomFx : MonoBehaviour
    {
        //全局UI mask动画的最大alpha值，以后只能改这里
        public static float  MaxMaskAlpha = 0.77f;
        [SerializeField] List<Animation> _subAnimations;
        [Header("如果不需要背景模糊效果则勾选此项")]
        [SerializeField] bool _disableBlurBg = false;
        public bool DisableBlurBg
        {
            get { return _disableBlurBg;}
            set { _disableBlurBg = value;}
        }
        [Header("如果不需要控制子节点Animation逐个激活则勾选此项")]
        public bool _IgnoreSubAnim = false;
        [Header("子动画播放是否要兼容item动态删减")]
        public bool _isDynamicSubAnimItems = false;
        [Header("子节点Animation逐个激活间隔时间")]
        [SerializeField] float interval = 0.05f;
        public float Interval
        {
            get { return interval; }
            set { interval = value; }
        }
        [SerializeField] CanvasGroup mask;
        public float AnimInterval
        {
            get => interval;
            set => interval = value;
        }
        /// <summary>
        /// 用于解决同时存在Animation和Animator的冲突情况，Animation动画播放时暂时禁用Animator
        /// </summary>
        List<Animator> _animators;

        /// <summary>
        /// 整个脚本动画播放完毕时的回调
        /// </summary>
        public Action _onAnimationDone;

        private void Awake()
        {
            ResetSpineAnimation();
            if (_subAnimations == null)
            {
                _subAnimations = new List<Animation>();
                _subAnimations.Clear();
            }
            _animators = new List<Animator>();
            _animators.Clear();

            Animation anm = GetComponent<Animation>();

            var masktf = transform.Find("mask");
            if (masktf == null) return;
            
            if (masktf.TryGetComponent(out mask))
            {   
                mask.alpha = 0.01f;
            }
        }
        void OnEnable()
        {
            if (mask != null) mask.alpha = 0.01f;
        }
        public void Setup()
        {

        }

                /// <summary>
        /// 用于处理Spine动画衔接播放
        /// </summary>
        [SerializeField]
        List<SkeletonGraphic> _spineGraphics;
        [SerializeField] float _spineAnimDelay = 0.33f;
        public IEnumerator QueuedPlaySpine(List<SkeletonGraphic> animations, float timeOffset)
        {
            yield return new WaitForSeconds(_spineAnimDelay);
            foreach (var item in animations)
            {
                if (item != null && item.gameObject.activeInHierarchy)
                {
                    item.timeScale = 1.0f;
                    string unlockName = "ani_unlock";
                    if (item.AnimationState.GetCurrent(0) != null) unlockName = item.AnimationState.GetCurrent(0).Animation.Name;
                    string idleName = "ani_idle";
                    if (unlockName.Contains("ani_unlock")) idleName = unlockName.Replace("ani_unlock", "ani_idle");
                    var data = item.SkeletonDataAsset.GetSkeletonData(false);

                    if (data.FindAnimation(unlockName) != null)
                    {
                        float delay = data.FindAnimation(unlockName).Duration;
                        if (data.FindAnimation(idleName) != null)
                            item.AnimationState.AddAnimation(0, idleName, true, delay);
                    }
                    else
                    {
                        Debug.LogWarning("当前spine动画不包含 ani_unlock 动画");
                    }
                    yield return new WaitForSeconds(timeOffset);
                }
                else
                {
                    Debug.LogWarning("当前spine动画为空");
                }
            }
            yield return null;
        }

        void ResetSpineAnimation()
        {
            if (_spineGraphics != null && _spineGraphics.Count > 0)
            {
                foreach (var item in _spineGraphics)
                {
                    if (item != null)
                    {
                        if(!item.startingAnimation.Contains("unlock"))
                        {
                            var data = item.SkeletonDataAsset.GetSkeletonData(false);   
                            var unlockClip = data.Animations.FirstOrDefault(x =>x.Name.Contains("unlock"));                 
                            item.startingAnimation = unlockClip != null?data.Animations.FirstOrDefault(x =>x.Name.Contains("unlock")).Name : string.Empty;
                        }
                        item.startingLoop = false;
                        item.Initialize(true);
                        item.timeScale = 0f;
                        // spine  初始化 override 重新生成mesh，会导致 Follower 脚本初始化失败，需要再次手动初始化
                        var boneFollowers = transform.GetComponentsInChildren<BoneFollowerGraphic>();
                        if (boneFollowers != null && boneFollowers.Length > 0)
                        {
                            foreach (var follwers in boneFollowers)
                            {
                                follwers.Initialize();
                            }
                        }
                    }
                }
            }
        }

        
        void MaskAnimation(bool isOpen = true)
        {
            if(mask == null) return;
            var curve = isOpen ? ArtUtility.IncreaseLinearCurve : ArtUtility.DecreaseLinearCurve;
            StartCoroutine(ArtAnimation.DoAnimation(0.33f , (value) =>
            {
                mask.alpha =  Mathf.Lerp(0.01f , MaxMaskAlpha , curve.Evaluate(value));
                //Mathf.Lerp(0.01f , MaxMaskAlpha , Mathf.Pow(curve.Evaluate(value), 2.2f));
            }));
        }

        public void MaskOpen()
        {
            MaskAnimation();
        }

        public void MaskClose()
        {
            MaskAnimation(false);
        }

        /// <summary>
        /// 只会保留命名里带有pop的Animation clip，制作动画时需要命名约束
        /// </summary>
        public void GetSubAnimations()
        {
            if (!_isDynamicSubAnimItems)
            {
                if (_subAnimations == null || _subAnimations.Count == 0)
                {
                    GetAnimationsInChildren(transform, out _subAnimations);
                }
            }
            else
            {
                GetAnimationsInChildren(transform, out _subAnimations);
            }
        }

        public void ClearSubAnimations()
        {
            if (_subAnimations != null || _subAnimations.Count > 0)
            {
                _subAnimations.Clear();
            }
        }

        RawImage _rawImage = null;
        /// <summary>
        /// 调用背景抓帧模糊效果，目前通过动画事件帧驱动
        /// </summary>
        public void SetActive()
        {
            try
            {
                if (!DisableBlurBg)
                {
                    if(_rawImage == null)
                    {
                        var blurGo = new GameObject("blurbg");
                        blurGo.transform.parent = transform;
                        blurGo.transform.SetAsFirstSibling();
                        blurGo.transform.Reset();
                        var rectTran = blurGo.AddComponent<RectTransform>();
                        rectTran.sizeDelta = new Vector2(Screen.width, Screen.height);
                        _rawImage = blurGo.AddComponent<RawImage>();
                        _rawImage.color = new Color(1 , 1, 1 , 0);
                    }
                    BgFromCamera.instance.PlayFullOpen(_rawImage);
                }
            }
            catch (Exception e)
            {
#if _ARTEST_PRESENTATION
                Debug.Log("ERROR:" + e.Message);
#endif
            }

        }

        /// <summary>
        /// 播放背景模糊关闭动画
        /// </summary>
        public void SetDisable()
        {
            if (!DisableBlurBg && _rawImage != null)
            {
                BgFromCamera.instance.PlayFullClose(_rawImage);
            }
        }

        /// <summary>
        /// 重置需要依次激活的动画节点，目前通过动画事件帧驱动
        /// </summary>
        public void ResetSubAnimations()
        {
            //_onAnimationDone = null;

            //尝试获取子节点Animation组件,不需要动画控制连续激活的子动画不操作
            if (!_IgnoreSubAnim)
            {
                GetSubAnimations();
                if (_subAnimations.Count > 0)
                {
                    SetSubAnimationObjects(_subAnimations, false);
                }

                //修复Animator冲突，临时禁用掉，动画结束的回调会重新激活
                DisableConflictAnimators();
                _onAnimationDone += EnableConflictAnimators;
            }
        }
        /// <summary>
        /// 重置layout，先把layout移动到屏幕外5000位置，激活所有Animation物件，得到正确的localPosition
        /// 下一帧再隐藏掉，把layout禁用，然后移动回到屏幕正确位置，防止动画播放的时候跟layout排布效果冲突
        /// layou会在动画播放完毕后由回调重新激活
        /// </summary>
        public void ResetLayout()
        {
            try
            {
                if (_subAnimations.Count > 0)
                {
                    List<LayoutGroup> layouts = new List<LayoutGroup>();
                    for (int i = 0; i < _subAnimations.Count; i++)
                    {
                        if (_subAnimations[i].transform.parent.TryGetComponent(out LayoutGroup layout))
                        {
                            if (!layouts.Contains(layout) && layout.enabled)
                            {
                                layouts.Add(layout);
                            }
                        }
                    }

                    if (layouts.Count > 0)
                    {
                        StartCoroutine(LayoutFix(layouts));
                    }
                }
                else
                {
                    return;
                }
            }
            catch (Exception e)
            {
#if _ARTEST_PRESENTATION
                Debug.LogWarning(">>>>>>>" + e.Message);
#endif
            }
        }
        /// <summary>
        /// 依次间隔激活效果，目前通过动画事件帧驱动
        /// </summary>
        public void PlayQueuedActive()
        {
            if (_subAnimations.Count > 0)
            {
                StartCoroutine(QueuedPlayAnimation(_subAnimations , interval , _onAnimationDone));
            }
        }

        public void PlayQueuedActiveWithLayout()
        {
            if (_subAnimations.Count > 0)
            {
                StartCoroutine(QueuedActiveAnimWithLayoutFix());
            }
        }

        public void PlayQueuedAnimations(List<Animation> animations , float timeOffset)
        {
            if (animations.Count > 0)
            {
                StartCoroutine(QueuedPlayAnimation(animations , timeOffset));
            }
        }

        void EnableLayouts(List<LayoutGroup> layputs)
        {
            if (layputs != null && layputs.Count > 0)
            {
                for (int i = 0; i < layputs.Count; i++)
                {
                    layputs[i].enabled = true;
                }
            }
        }

        void SetSubAnimationObjects(List<Animation> _subAnims , bool value)
        {
            try
            {
                if (_subAnims.Count >0)
                {
                    foreach (var item in _subAnims)
                    {
                        if (item != null)
                        {
                            if (item.isPlaying) item.Stop();
                            if (item.playAutomatically) item.playAutomatically = false;
                            //防止pop动画第一帧跳帧，起始帧scale为0
                            if (!value)
                            {
                                item.transform.localScale = Vector3.zero;
                            }
                            else
                            {
                                item.transform.localScale = Vector3.one;
                            }
                        }
                        else
                        {
                            _subAnims.Remove(item);
                        }
                    }
                }
            }
            catch (Exception e)
            {
#if _ARTEST_PRESENTATION
                Debug.LogWarning(e.Message);
#endif
            }
        }
        public void PlayParticles()
        {
            var ps = transform.GetComponentsInChildren<ParticleSystem>(true);
            if (ps.Length>0)
            {
                ArtUtility.EnableObjects(ps);
                ArtUtility.StopParticles(ps);
                ArtUtility.PlayParticles(ps);
            }
        }

        private void  GetAnimationsInChildren(Component component , out List<Animation> animations)
        {
            animations = new List<Animation>();
            animations.Clear();

            var anis = component.GetComponentsInChildren<Animation>(true);
            for (int i = 0; i < anis.Length; i++)
            {
                if (anis[i].clip != null)
                {
                    if (anis[i].clip.name.Contains("pop"))
                    {
                        animations.Add(anis[i]);
                    }
                }
            }
        }

        void EnableConflictAnimators()
        {
            if (_animators.Count > 0)
            {
                foreach (var item in _animators)
                {
                    item.enabled = true;
                }
            }
        }

        void DisableConflictAnimators()
        {
            _animators.Clear();
            foreach (var item in _subAnimations)
            {
                if (item.TryGetComponent(out Animator _animator))
                {
                    _animators.Add(_animator);

                    _animator.enabled = false;
                }
            }
        }

        /// <summary>
        /// 事件帧驱动方法
        /// </summary>
        /// <param name="_disable">事件帧只支持浮点整形和字符串类型的参数，这里用0表示false，1表示true</param>
        public void StopParticles(int _disable = 0)
        {
            var ps = transform.GetComponentsInChildren<ParticleSystem>(true);
            
            if (ps.Length > 0)
            {
                if (_disable == 1)
                {
                    ArtUtility.DisableObjects(ps);
                    return;
                }
                ArtUtility.StopParticles(ps);
            }
        }


#if _ARTEST_PRESENTATION
        public void MovePopItemsToRoot()
        {

        }
#endif

        [SerializeField] bool visiableCheck = false;
        public IEnumerator QueuedPlayAnimation(List<Animation> animations , float timeOffset ,Action callback = null)
        {
            if (animations == null)
            {
                callback?.Invoke();
                yield break;
            }
            foreach (var item in animations)
            {
                if (item != null && item.gameObject.activeInHierarchy)
                {
                    var canPlay = visiableCheck?ArtUtility.CheckElementInsideScreen(UICanvasAdapter.CurrentCanvas.worldCamera, item.gameObject) : true;

                    if(canPlay)
                    {
                        item.Play();
                        yield return new WaitForSeconds(timeOffset);
                    }
                    else
                    {
                        //SetSubAnimationObjects(animations, true);
                        item.transform.localScale = Vector3.one;
                    }
                }
            }
#if _ARTEST_PRESENTATION
            yield return new WaitForSeconds(animations[animations.Count-1].clip.length);
#endif
            callback?.Invoke();
        }

        public IEnumerator QueuedActivePlayAnimation(List<Animation> animations, Action callback = null)
        {
            if (animations == null)
            {
                callback?.Invoke();
                yield break;
            }
            foreach (var item in animations)
            {
                if (item != null)
                {
                    item.gameObject.SetActive(true);
                    item.Play();
                    yield return new WaitForSeconds(interval);
                }
            }
            callback?.Invoke();
        }


        public IEnumerator LayoutFix(List<LayoutGroup> layouts)
        {
            _onAnimationDone += new Action(() => EnableLayouts(layouts));

            List<Animation> aniList = new List<Animation>();
            foreach (var item in layouts)
            {
                GetAnimationsInChildren(item , out aniList);
                item.GetComponent<RectTransform>().anchoredPosition += new Vector2(5000f, 0f);
                item.enabled = true;
                SetSubAnimationObjects(aniList, true);
            }
            yield return new WaitForEndOfFrame();

            foreach (var item in layouts)
            {
                GetAnimationsInChildren(item, out aniList);
                item.enabled = false;
                SetSubAnimationObjects(aniList, false);

                item.GetComponent<RectTransform>().anchoredPosition += new Vector2(-5000f, 0f);
            }
            
        }

        public IEnumerator QueuedActiveAnimWithLayoutFix()
        {
            if (_subAnimations.Count > 0)
            {
                List<ScrollRect> scrollRects = transform.GetComponentsInChildren<ScrollRect>().ToList();
                List<LayoutGroup> layouts = new List<LayoutGroup>();
                for (int i = 0; i < _subAnimations.Count; i++)
                {
                    if (_subAnimations[i].transform.parent.TryGetComponent(out LayoutGroup layout))
                    {
                        if (!layouts.Contains(layout) && layout.enabled)
                        {
                            layouts.Add(layout);
                        }
                    }
                }

                if (scrollRects.Count > 0)
                {
                    foreach (var scroll in scrollRects)
                    {
                        scroll.enabled = false;
                    }
                }

                if (layouts.Count > 0)
                {
                    yield return StartCoroutine(LayoutFix(layouts));
                }

                if (scrollRects.Count > 0)
                {
                    foreach (var scroll in scrollRects)
                    {
                        scroll.enabled = true;
                    }
                }
            }
            else
            {
                yield break;
            }
            
            yield return null;
            yield return StartCoroutine(QueuedPlayAnimation(_subAnimations, interval, _onAnimationDone));
        }
    }

}