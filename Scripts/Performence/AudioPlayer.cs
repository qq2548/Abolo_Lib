using System;
using System.Collections.Generic;
using UnityEngine;
using Study_Farm;

namespace AboloLib
{
    public class AudioPlayer : AboloSingleton<AudioPlayer>, IAudioAdapter
    {
        [SerializeField] AudioSource _audioSource;
        public AudioSource Audio_Source
        {
            get
            {
                return _audioSource;
            }
            set
            {
                _audioSource = value;
            }
        }

        protected Dictionary<string, AudioClip> audioDic;
        /// <summary>
        /// 音效字典
        /// </summary>
        public Dictionary<string, AudioClip> AudioDic
        {
            get => audioDic;
            set => audioDic = value;
        }

        public override void Init()
        {
            base.Init();
            AudioDic = AudioPlayerAdapter.AudiosDic;
        }

        public void PlayAudio(string audioName)
        {
            if (Audio_Source != null)
            {
                Audio_Source.PlayOneShot(AudioDic[audioName]);
            }
#if _ARTEST_PRESENTATION
            else
            {
                Debug.Log(ArtUtility.WarningLog + this.name + " s'MyAudioSource 未赋值");
            }
#endif
        }
    }
}
