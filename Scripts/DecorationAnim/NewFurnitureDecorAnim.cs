using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace AboloLib
{
    public class NewFurnitureDecorAnim : DecorationAnim
    {
        //[Header("如果一个节点只有一个SpriteRenderer且没有Mesh在场，生成的碰撞体Mesh是无效的")]

        //用于装修节点选中悬浮高亮效果的演示
        public static List<NewFurnitureDecorAnim> Instances;
        [SerializeField] int _mySelectID;
        private void OnDisable()
        {
            if (Instances!=null && Instances.Contains(this))
            {
                Instances.Remove(this);
            }
        }


        [Header("装修完成时的粒子特效预制体")]
        [SerializeField] protected DecorationParticle decorationParticle;
        public DecorationParticle DoneFx
        {
            get => decorationParticle;
            set => decorationParticle = value;
        }
        [Header("装修完成特效粒子数量权重值，最大值为5，生成约150个粒子")]
        [SerializeField] protected float _particleAmount = 1.0f;
        [Header("选中预览时是否漂浮")]
        [SerializeField] protected bool _floating = true;
        public bool Floating
        {
            get => _floating;
            set => _floating = value;
        }

        MaterialPropertyBlock _propertyBlock;

        bool _selecting = false;

        public bool Selecting
        {
            get
            {
                return _selecting;
            }
            set
            {
                _selecting = value;
#if _ARTEST_PRESENTATION
                if (value)
                {
                    PlaySelectedAnimation();
                }
#endif
            }
        }

        public override void Play()
        {
            //播放一个BranNew音效
            if(_playMyAudio)
            {
                AudioPlayerAdapter.PlayAudio("hotspot_BrandNew");
            }

            Action callback = new Action(PlayDecorDoneFx);
//#if _ARTEST_PRESENTATION
//            callback += SetCollider;
//#endif
            StartCoroutine(MultiGraphicFadeInFixed(CurveAdapter.CurveFactory.durationPreset3 , callback));
        }


        public override void SetUp()
        {
            base.SetUp();
            HideSelf();
            ResetSubItems(factor: 0f);
            _propertyBlock = new MaterialPropertyBlock();
            //SetCollider();

            if (Instances == null)
            {
                Instances = new List<NewFurnitureDecorAnim>();
                Instances.Clear();
            }
            Instances.Add(this);
            _mySelectID = Instances.IndexOf(this);
        }

        private void SetCollider()
        {
            if (transform.TryGetComponent<MeshCollider>(out MeshCollider mc))
            {
                if (mc.sharedMesh!=null)
                {
                    mc.sharedMesh.Clear();
                }
                mc.sharedMesh = ArtUtility.CreateCombinedMesh(transform , transform.worldToLocalMatrix);
            }
        }

        public override void ShowSelf()
        {
            if (Application.isPlaying)
            {
                base.ShowSelf();
                SetCollider();
            }
        }

        /// <summary>
        ///新增类装修动画周期赋值
        /// </summary>
        public override void SetUnlockDuration()
        {
            //这个1.5应该是为了粒子动画播放完毕预留的延迟
            _myUnlockDuration = 1.5f
                + CurveAdapter.CurveFactory.durationPreset3 * (1.0f + _decorItems.Length * _interval)
                + CurveAdapter.CurveFactory.durationPreset3 * (1.0f + _popItems.Length * _interval);
        }




        /// <summary>
        /// 装修完毕播放粒子动画
        /// </summary>
        public void PlayDecorDoneFx()
        {
            ResetAnimator();

            if (decorationParticle != null)
            {
                var dp = Instantiate(decorationParticle, transform);
                foreach (var item in dp._particleSystems)
                {
                    item.weight = _particleAmount;
                }
                dp.EmitDecorationParticle(transform);
                //演示装修动画音效
                //DecorationAniamtionTest.instance.PlayAudioClip(this._myAudioName);//本地的方法，废弃
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
                Action callback = new(() => DestroyImmediate(dp.gameObject));
                callback += OnAnimationDone;
                // StopAnimation();
                ani = StartCoroutine(ArtAnimDelayCoroutine(1.5f, callback));
            }
            else
            {
                Debug.Log("缺少对应的特效引用");
            }
        }

#if _ARTEST_PRESENTATION
        public void PlaySelectedAnimation()
        {
            StartCoroutine(SelectAnimation());
        }

        IEnumerator SelectAnimation()
        {
            Coroutine coroutine;

            Renderer[] renders = transform.GetComponentsInChildren<Renderer>();

            while (Selecting)
            {
                yield return coroutine = StartCoroutine(SelectingAnimation(renders,1.0f));
            }
            yield return StartCoroutine(SelectedAnimation(renders,0.5f, 
                CurveAdapter.AnimCurveDic[CurveFactory.CurveType.SlowSteady]));
        }

        IEnumerator SelectingAnimation(Renderer[] renderers , float duration , AnimationCurve curve = null, Action callback = null)
        {
            curve = curve == null ? CurveAdapter.AnimCurveDic[CurveFactory.CurveType.FloatingPosition] : curve;

            float timer = 0.0f;
            Vector3 fv;
            if (_floating)
            {
                fv = Vector3.up;
            }
            else
            {
                fv = Vector3.zero;
            }
            while (timer <= 1.0f)
            {
                timer += Time.deltaTime / duration;
                root.localPosition = curve.Evaluate(timer) * 0.5f * fv;
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].GetPropertyBlock(_propertyBlock);
                    //曝光浮动范围1.0~1.2
                    _propertyBlock.SetFloat("_Exposure", curve.Evaluate(timer) * 2f - 1f);
                    renderers[i].SetPropertyBlock(_propertyBlock);
                }
                yield return null;
            }

        }

        IEnumerator SelectedAnimation(Renderer[] renderers , float duration, AnimationCurve curve = null, Action callback = null)
        {
            curve = curve == null ? CurveAdapter.AnimCurveDic[CurveFactory.CurveType.FlyPosition] : curve;
            float timer = 0.0f;
            Vector3 _startPos = root.localPosition;
            while (timer <= 1.0f)
            {
                timer += Time.deltaTime / duration;
                root.localPosition =Vector3.Lerp(_startPos , Vector3.zero , curve.Evaluate(timer));
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetFloat("_Exposure", Mathf.Lerp(1.1f, 1.0f, curve.Evaluate(timer)));
                    renderers[i].SetPropertyBlock(_propertyBlock);
                }
                yield return null;
            }
            callback?.Invoke();
        }
#endif

        public IEnumerator MultiGraphicFadeInFixed(float duration, Action callback = null)
        {
            //anim_items 子节点入场动画
            yield return StartCoroutine(DoAnimationWithInterval(_decorItems.Length, duration , _interval , MultiFadeDeltaAnimation(_decorItems, _startPositions,_interval, true)));
            //pop_items 子节点缩放动画
            if (pop_root != null && _popItems != null && _popItems.Length > 0)
            {
                if(!pop_root.gameObject.activeInHierarchy) pop_root.gameObject.SetActive(true);
                yield return StartCoroutine(DoAnimationWithInterval(_popItems.Length, CurveAdapter.CurveFactory.durationPreset3,
                    _interval , MultiPopDeltaAnimation(_popItems , _interval ,true)));
            }
            //装修动画结束时回调
            callback?.Invoke();
        }

    }

}