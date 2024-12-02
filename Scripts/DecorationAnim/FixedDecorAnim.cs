using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace AboloLib
{
    public class FixedDecorAnim : ClearDecorAnim
    {
        [Header("修复特效预制体")]
        [SerializeField] DecorationParticle _fixFx;
        public DecorationParticle FixFx
        {
            get => _fixFx;
            set => _fixFx = value;
        }
        [Header("修复特效播放后延迟多少s播放装修完成粒子")]
        [SerializeField] float _fixFxStopDelay;
        public float FixFxStopDelay
        {
            get => _fixFxStopDelay;
            set => _fixFxStopDelay = value;
        }
       // [SerializeField] bool _hideSelf;

        public override void SetUnlockDuration()
        {
            //修复效果需要特效停止时就切换成新装修节点，这像衔接效果更平滑
            _myUnlockDuration = _fixFxStopDelay;
        }

        public override void Play()
        {
            base.Play();
            DecorationParticle dp = Instantiate(_fixFx, transform);
            dp.Play();
            //演示装修动画音效
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

            StopAnimation();
            ani = StartCoroutine(ArtAnimDelayCoroutine(_fixFxStopDelay, () => dp.Stop()));
            StartCoroutine(ArtAnimDelayCoroutine(_fixFxStopDelay + 1.0f, () => DestroyImmediate(dp.gameObject)));
        }

    }
}