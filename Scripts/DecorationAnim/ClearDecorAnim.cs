using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace AboloLib
{

    public class ClearDecorAnim : DecorationAnim
    {
        [Header("清扫特效预制体")]
        [SerializeField] GameObject _clearFx;
        public GameObject ClearFx
        {
            get => _clearFx;
            set => _clearFx = value;
        }
        [SerializeField] float _mClearFxDuration = 1.5f;

        public override void SetUp()
        {
            base.SetUp();
            ShowSelf();
            //防止万一出现清扫特效时长短过unlock动画clip时长
            if (_clearFx != null)
            {
                float t = GetMyUnlockClipLength();
                if (_mClearFxDuration < t)
                {
                    _mClearFxDuration = t + 0.1f;
                }
            }
        }
        public override void ResetSubItems(float factor)
        {
            base.ResetSubItems(factor);
            if(_spineAnims.Length > 0)
            {
                foreach (var anim in _spineAnims)
                {
                    anim.Initialize(true);
                    if(factor == 0f)
                    {
                        anim.AnimationState.SetAnimation(0, "ani_idle", false);
                    }
                    else
                    {
                        anim.AnimationState.SetAnimation(0, "ani_init", true);
                    }
                    //anim.Update(0.2f);
                }
            }
        }
        /// <summary>
        ///清扫类装修动画周期赋值
        /// </summary>
        public override void SetUnlockDuration()
        {
            //清扫特效时长默认大于unlock动画clip时长
            if (_clearFx != null)
            {
                _myUnlockDuration = MathF.Max(CurveAdapter.CurveFactory.durationPreset2 * (1.0f + _decorItems.Length * Interval)
                                                                    + CurveAdapter.CurveFactory.durationPreset3 * (1.0f + _popItems.Length * Interval),
                                                                      _mClearFxDuration);
            }
            else
            {
                _myUnlockDuration = MathF.Max(CurveAdapter.CurveFactory.durationPreset2 * (1.0f + _decorItems.Length * Interval)
                                                                    + CurveAdapter.CurveFactory.durationPreset3 * (1.0f + _popItems.Length * Interval),
                                                                        GetMyUnlockClipLength());
            }
        }

        public override void Play()
        {
            ShowSelf();
            if (_clearFx != null)
            {
                PlayFx(_clearFx);
            }

            StartCoroutine(UnlockSpineAnimations());

            //StartCoroutine(ArtAnimDelayCoroutine(_myUnlockDuration, new Action(() => HideSelf())));
            if (_decorItems.Length > 0)
            {
                StartCoroutine(MultiGraphicFadeOutFixed(
                    CurveAdapter.CurveFactory.durationPreset2 , () =>
                    {
                        if (static_root != null) static_root.gameObject.SetActive(false);
                    }));
            }
        }


        public IEnumerator MultiGraphicFadeOutFixed( float duration , Action callback = null)
        {

            //ani_items 子节点消失动画
            yield return StartCoroutine(DoAnimationWithInterval(_decorItems.Length, duration,Interval ,MultiFadeDeltaAnimation(_decorItems , _startPositions , Interval ,false)));
            //pop_items 子节点消失动画
            if (pop_root != null && _popItems != null && _popItems.Length > 0)
            {
                if (!pop_root.gameObject.activeInHierarchy) pop_root.gameObject.SetActive(true);
                yield return StartCoroutine(DoAnimationWithInterval(_popItems.Length , Interval , CurveAdapter.CurveFactory.durationPreset3 , 
                    MultiPopDeltaAnimation(_popItems , Interval , false) ));
            }
            callback?.Invoke();
        }

    }
}