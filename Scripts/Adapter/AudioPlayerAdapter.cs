using System;
using System.Collections;
using System.Collections.Generic;
using AboloLib;
using UnityEngine;

public interface IAudioAdapter
{
    void PlayAudio(string audioName);
}
/// <summary>
/// 音效播放：所有音效播放通过这个类引用,目前只提供播放单独某个音效的类
/// </summary>
public static class AudioPlayerAdapter
{
    private static IAudioAdapter mAdapter;
    public static Func<Dictionary<string, AudioClip>> GetAudioDic;
    public static void Init(IAudioAdapter adapter, Func<Dictionary<string, AudioClip>> func)
    {
        mAdapter = adapter;
        GetAudioDic = func;
    }

    public static Dictionary<string , AudioClip> AudiosDic
    {
        get => GetAudioDic?.Invoke();
    }

    public static void  PlayAudio(string audioName)
    {
        if (mAdapter != null)
        {
            mAdapter.PlayAudio(audioName);
        }
    }
}
