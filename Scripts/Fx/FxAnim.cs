using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace AboloLib
{
    public class FxAnim : ArtAnimation,IFxAnimCtrl
    {
        protected Transform root;
        /// <summary>
        /// 执行停止播放时延迟多久关闭此特效
        /// </summary>
        [Tooltip("停止播放以后延迟多久关闭此特效")]
        [SerializeField] protected float delay = 1.0f;
        [SerializeField] bool _playOnEnable = false;
        protected virtual void Awake()
        {
            root = transform.Find("main_root");
            SetRootActive(false);
        }

        private void OnEnable()
        {
            if (_playOnEnable)
            {
                Play();
            }
            else
            {
                SetRootActive(false);
            }
        }

        protected virtual void OnInit()
        {

        }

        public override void Init() 
        {
            base.Init(); 
            OnInit();

        }

        public virtual  void Play()
        {
            SetRootActive(true);
        }


        protected void SetRootActive(bool isActive)
        {
            if (root != null)
            {
                root.gameObject.SetActive(isActive);
#if _ARTEST_PRESENTATION
                if (!isActive)
                {
                    //var p = transform.parent;
                    //string s;
                    //s = p ? p.name : "No Parent";
                    //Debug.Log(s + "Stopped" + "特效名字：" + name);
                }
#endif
            }
            else
            {
#if _ARTEST_PRESENTATION
                Debug.LogWarning(ArtUtility.WarningLog + "特效名字：" + name + " -----main_root 节点不存在，请检查资源");
#endif
                return;

            }
        }

        public void StopImmediate()
        {
            StopAnimation();
            SetRootActive(false);
        }

        public virtual void Stop(Action callback = null)
        {
            try
            {
                StopAnimation();
                var ps = GetComponentsInChildren<ParticleSystem>();
                if (ps != null)
                {
                    ArtUtility.StopParticles(ps);
                }

                if (callback != null)
                {
                    if (this.gameObject.activeInHierarchy) ani = StartCoroutine(ArtAnimDelayCoroutine(delay, callback));
                }
                else
                {
                    if (this.gameObject.activeInHierarchy)
                        ani = StartCoroutine(ArtAnimDelayCoroutine(delay, new Action(() => SetRootActive(false))));
                }
            }
            catch (Exception e)
            {
                Debug.Log("----此特效不包含粒子系统组件" + e.Message);
            }
        }

#if _ARTEST_PRESENTATION
        public void TestPlay()
        {
            Play();
            StartCoroutine(ArtAnimDelayCoroutine(delay, () => Stop(null)));
        }
#endif
    }
}