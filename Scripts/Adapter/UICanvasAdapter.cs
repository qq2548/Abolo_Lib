using System;
using System.Collections;
using System.Collections.Generic;
using AboloLib;
using UnityEngine;

/// <summary>
/// UI RootCanvas
/// </summary>
public static class UICanvasAdapter
{
    private static Func<Canvas> canvasFunc;

    public static void Init(Func<Canvas> func)
    {
        canvasFunc = func;
    }

    public static Canvas CurrentCanvas
    {
        get { return canvasFunc?.Invoke(); }
    }
}
