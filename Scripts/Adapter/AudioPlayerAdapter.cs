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
    public static Func<IAudioAdapter> GetAudioAdapter;
    public static void Init(Func<IAudioAdapter> func)
    {
        GetAudioAdapter = func;
    }

    public static IAudioAdapter Adapter
    {
        get => GetAudioAdapter?.Invoke();
    }


    public static void  PlayAudio(string audioName)
    {
        if (Adapter != null) Adapter.PlayAudio(audioName);
    }


    public static void PlayBGM(string audioName)
    {
        if (Adapter != null)
        {
            Adapter.PlayBgm(audioName);
        }
    }
}
