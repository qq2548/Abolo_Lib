using System;
using System.Collections;
using System.Collections.Generic;
using AboloLib;
using UnityEngine;

/// <summary>
/// 游戏相机获取，适配类
/// </summary>
public static class GameCameraAdapter
{
    private static Func<Camera> cameraFunc;

    public static void Init(Func<Camera> func)
    {
        cameraFunc = func;
    }

    public static Camera CurrentCamera
    {
        get { return cameraFunc?.Invoke(); }
    }
}
