using System;
using System.Collections;
using System.Collections.Generic;
using AboloLib;
using UnityEngine;

public interface IAudioAdapter
{
    void PlayAudio(string audioName);
    void PlayBgm(string bgmName);
}
/// <summary>
/// 音效播放：所有音效播放通过这个类引用,目前只提供播放单独某个音效的类
/// </summary>
public static class AudioPlayerAdapter
{
    //private static IAudioAdapter mAdapter;
    public static Func<Dictionary<string, AudioClip>> GetAudioDic;
    public static Func<AudioSource> GetSoundAudioSource;
    public static Func<AudioSource> GetBgmAudioSource;

    private static float mainVolume = 1.0f;
    public static float MainAudionVolume
    {
        get => mainVolume;
        set
        {
            mainVolume = value;
            SoundAudioSource.volume = SoundAudionVolume * value;
            BgmAudioSource.volume = BgmAudionVolume * value;
        }
    }
    private static float soundVolume = 0.5f;
    public static float SoundAudionVolume
    {
        get => soundVolume;
        set
        {
            soundVolume = value;
            SoundAudioSource.volume = soundVolume * MainAudionVolume;
        }
    }
    private static float bgmVolume = 0.5f;
    public static float BgmAudionVolume
    {
        get => bgmVolume;
        set
        {
            bgmVolume = value;
            BgmAudioSource.volume = bgmVolume * MainAudionVolume;
        }
    }

    public static void Init( Func<Dictionary<string, AudioClip>> func , Func<AudioSource> func1 , Func<AudioSource> func2)
    {
        GetAudioDic = func;
        GetSoundAudioSource = func1;
        GetBgmAudioSource = func2;
    }

    public static Dictionary<string , AudioClip> AudiosDic
    {
        get => GetAudioDic?.Invoke();
    }

    public static AudioSource SoundAudioSource
    {
        get => GetSoundAudioSource?.Invoke();
    }

    public static AudioSource BgmAudioSource
    {
        get => GetBgmAudioSource?.Invoke();
    }

    public static void  PlayAudio(string audioName)
    {
        if (SoundAudioSource != null) SoundAudioSource.PlayOneShot(AudiosDic[audioName]);
    }

    public static void PlayBGM(string audioName)
    {
        if (BgmAudioSource != null)
        {
            BgmAudioSource.clip = AudiosDic[audioName];
            BgmAudioSource.Play();
        }
    }
}
