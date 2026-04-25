using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

namespace AboloLib
{
    public delegate void OnDecorationDone();


    [RequireComponent(typeof(Animator))] [DisallowMultipleComponent] 
    public class DecorationAnim : ArtAnimation, IDecorAnimCtrl
    {
        private static float AnimSpeed= 1.0f;
        public static float SPEED
        {
            get
            {
                if(AnimSpeed > 0)
                {
                    return AnimSpeed;
                }
                else
                {
                    return 0.0001f;
                }
            }   
            set
            {
                if(value > 0)
                {
                    AnimSpeed = value;
                }
                else
                {
                    Debug.LogError("AnimSpeed can not be equal or less than 0!!");
                }
            }
        }
        [Header("每个物件开始动画之间的时间间隔")]
        [SerializeField] protected float _interval = 0.1f;
        public float Interval
        {
            get => _interval/SPEED;
            set => _interval = value;
        }

        [SerializeField] protected Renderer[] _decorItems;
        [SerializeField] protected Renderer[] _popItems;
        [SerializeField] protected SkeletonAnimation[] _spineAnims;
        [Header("是否播放音效")]
        [SerializeField] protected bool _playMyAudio = true;
        protected Vector3[] _startPositions;
        //protected bool _unlocked = false;
        protected Transform root;
        protected Transform spr_root;
        protected Transform pop_root;
        protected Transform spine_root;
        protected Transform static_root;

        /// <summary>
        /// 这里保存一个音效名称，后续添加音效同步演示播放工能，这里字段用于读取资源，给程序同步资源和延迟信息
        /// </summary>
        public string _myAudioName;
        public float _myAudioPlayDelay;

         protected float _myUnlockDuration;

        protected bool _settled = false;

        public virtual float MyUnlockAnimDuration
        {
            get
            {
                return _myUnlockDuration;
            }
            set
            {
                _myUnlockDuration = value;
            }
        }

        public OnDecorationDone onDecorationDone;

        public virtual void Play()
        {

        }

        protected void OnAnimationDone()
        {
            onDecorationDone?.Invoke();
        }

        public virtual void HideSelf()
        {
            var go = root.gameObject;
            if (go.activeSelf)
            {
                go.SetActive(false);
            }
            root.transform.localPosition = Vector3.zero;
        }

        public virtual void ShowSelf()
        {
            if (!_settled)
            {
                SetUp();
            }
            var go = root.gameObject;
            if (!go.activeSelf)
            {
                go.SetActive(true);
            }
            root.transform.localPosition = Vector3.zero;
            ResetSubItems(factor: 1f);
            FixAnimationPlay();
        }

        void FixAnimationPlay()
        {
            var anis = transform.GetComponentsInChildren<Animation>(true);
            foreach (var item in anis)
            {
                string aniName = item.clip.name;
                if (aniName != null)
                {
                    if (!item.IsPlaying(aniName) && item.isActiveAndEnabled)
                    {
                        item.Play(aniName);
                    }
                }
            }
        }

        public virtual void SetUp()
        {
            onDecorationDone = null;
            root = transform.Find("root");
            spr_root = transform.Find("root/anim_items");

            if (spr_root == null)
            {
#if _ARTEST_PRESENTATION
                Debug.LogWarning(ArtUtility.WarningLog + transform.parent.name + "---" + name + "---spr_root---can not find!!!");
#endif
            }
            else
            {
                _decorItems = spr_root.GetComponentsInChildren<Renderer>();
                _startPositions = new Vector3[_decorItems.Length];
                for (int i = 0; i < _decorItems.Length; i++)
                {
                    _startPositions[i] = _decorItems[i].transform.position;
                }
            }

            pop_root = transform.Find("root/scale_items/pop_items");
            if (pop_root == null)
            {
#if _ARTEST_PRESENTATION
                Debug.LogWarning(ArtUtility.WarningLog + transform.parent.name + "---" + name + "---pop_root---can not find!!!");
#endif
            }
            else
            {
                _popItems = pop_root.GetComponentsInChildren<Renderer>(true);
            }
            //存储装修动画播放完毕需要的总时长
            SetUnlockDuration();
            _settled = true;
        }

        public override void Start()
        {
            base.Start();
#if _ARTEST_PRESENTATION
            //前端有自己的调用时机，这里只做测试演示用
            SetUp();
#endif
        }


        /// <summary>
        ///装修动画周期赋值，子类重写
        /// </summary>
        public virtual void SetUnlockDuration()
        {

        }

        public void PlayFx(GameObject fx)
        {
            if (fx == null)
            {
#if _ARTEST_PRESENTATION
                Debug.Log("没有可以播放的特效");
#endif
                return;
            }
            var _myFx = Instantiate(fx);
            _myFx.SetActive(false);
            _myFx.transform.localPosition = transform.position;
            //设置粒子播放速度
            ArtUtility.SetParticlesPlaySpeed(_myFx , SPEED);

            _myFx.SetActive(true);

            //一定延迟后销毁装修特效
            StartCoroutine(ArtAnimDelayCoroutine(CurveAdapter.CurveFactory.durationPreset7, 
                                                                            new Action(() => DestroyImmediate(_myFx))));

            //演示装修动画音效
            //播放音效
            //DecorationAniamtionTest.instance.PlayAudioClip(this);//本地的方法，废弃
            if (_playMyAudio)
            {
                try
                {
                    AudioPlayerAdapter.PlayAudio(this._myAudioName);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(_myAudioName + "音效丢失" + e.Message);
                }
            }
            
        }

        /// <summary>
        /// 重置需要动画解锁的物件
        /// </summary>
        /// <param name="factor">这个浮点参数用来决定是显示还是隐藏，0是隐藏(scale和alpha置为0)，1是显示(scale和alpha置为1)</param>
        public virtual void ResetSubItems(float factor)
        {
            root.gameObject.SetActive(true);
            if (pop_root != null)
            {
                pop_root.gameObject.SetActive(true);
                if (_popItems.Length>0)
                {
                    foreach (var item in _popItems)
                    {
                        item.transform.localScale = Vector3.one * factor;
                    }
                }
            }
            if (_decorItems.Length >0)
            {
                for (int i = 0; i < _decorItems.Length; i++)
                {
                    //_decorItems[i].color = new Color(_decorItems[i].color.r, _decorItems[i].color.g, _decorItems[i].color.b, factor);
                    _decorItems[i].TryGetComponent(out SpriteRenderer spriteRenderer);
                    if (spriteRenderer != null)
                        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, factor);
                    _decorItems[i].transform.position = _startPositions[i];
                }
            }
        }

        public void ResetAnimator()
        {
            transform.GetComponent<Animator>().Play("Default");
        }

        public float GetMyUnlockClipLength()
        {
            var clips = transform.GetComponent<Animator>().runtimeAnimatorController.animationClips;
            var mclip = clips.FirstOrDefault(s => (s.name == "Unlock"));
            return mclip.length;
        }

        /// <summary>
        /// 重置子节点Spine动画
        /// </summary>
        protected void InitSkeletonAnimations()
        {
            var sps = GetComponentsInChildren<SkeletonAnimation>(true);
            if (sps !=null && sps.Length >0)
            {
                foreach (var item in sps)
                {
                    if (item != null && item.gameObject.activeInHierarchy)
                    {
                        item.Initialize(true);
                    }
                }
            }
        }

        protected float GetSpineUnlockDelay()
        {
            float result = 0.0f;
            if (_spineAnims == null || _spineAnims.Length == 0) return result;
            for (int i = 0; i < _spineAnims.Length; i++)
            {
                float tmp = _spineAnims[i].GetClipDuration("ani_unlock") + i  * Interval;
                result = result < tmp? tmp : result;
            }
            return result;
        }

        public void PlaySpine(string clipName)
        {
            if (_spineAnims != null && _spineAnims.Length > 0)
            {
                foreach (var anim in _spineAnims)
                {
                    anim.Play(clipName);
                    if(clipName.Contains("unlock"))
                    {
                        float delay = anim.SkeletonDataAsset.GetSkeletonData(false).FindAnimation(clipName).Duration;
                        string name = clipName.Replace("unlock", "idle");
#if _ARTEST_PRESENTATION
                        Debug.Log("8888888888888**********" + name);
#endif
                        anim.AnimationState.AddAnimation(0 , name , true , delay);
                    }
                }
            }
        }

        public IEnumerator UnlockSpineAnimations()
        {
            yield return null;
            if (_spineAnims != null && _spineAnims.Length > 0)
            {
                foreach (var anim in _spineAnims)
                {
                    anim.timeScale = SPEED;
                    anim.Initialize(true);
                    anim.AnimationState.SetAnimation(0 , "ani_unlock" , false);
                    float delay = anim.SkeletonDataAsset.GetSkeletonData(false).FindAnimation("ani_unlock").Duration;
                    anim.AnimationState.AddAnimation(0 , "ani_idle", true , delay);
                    yield return new WaitForSeconds(Interval);
                }
            }
            yield return null;
        }

#if _ARTEST_PRESENTATION
        #region 编辑器方法
        /// <summary>
        /// 所有子节点Sprite Z轴随机偏移节点z轴
        /// </summary>
        public void RandomOffsetZaxis()
        {
            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var item in renderers)
            {
                if (item.sortingOrder != 0)
                {
                    Vector3 oriPos = item.transform.localPosition;
                    float offset = UnityEngine.Random.Range(0.0f, 0.5f);
                    item.transform.localPosition = new Vector3(oriPos.x, oriPos.y, offset);
                }
            }
        }

        /// <summary>
        /// 所有子节点Sprite Z轴随 SortingOrder 值 偏移z轴
        /// </summary>
        public void OffsetZaxisBySortingOrder()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            Vector3 offset = Vector3.one * 0.025f;
            //如果 SortingOrder 已为0则不修改
            foreach (var item in renderers)
            {
                if (item.sortingOrder != 0)
                {
                    Vector3 oriPos = item.transform.localPosition;
                    item.transform.localPosition = new Vector3(oriPos.x, oriPos.y, offset.z * item.sortingOrder);
                    item.sortingOrder = 0;
                }
            }
        }

        /// <summary>
        /// 所有子节点Sprite复位Z轴偏移值
        /// </summary>
        public void ResetZaxis()
        {
            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var item in renderers)
            {
                Vector3 oriPos = item.transform.localPosition;
                float offset = UnityEngine.Random.Range(0.0f, 0.5f);
                item.transform.localPosition = new Vector3(oriPos.x, oriPos.y, 0f);
            }
        }
        #endregion
#endif
    }

}