using System;
using System.Collections;
using System.Collections.Generic;
using AboloLib;
using UnityEngine;
/// <summary>
/// 曲线转接器，所有动画曲线和时间预设通过这个类来引用
/// </summary>
public class CurveAdapter
{
    private static Func<Dictionary<CurveFactory.CurveType , AnimationCurve>> GetAnimCurveDic;
    private static Func<CurveFactory> GetCurveFactory;

    public static void Init(Func<Dictionary<CurveFactory.CurveType, AnimationCurve>> func1 , Func<CurveFactory> func2)
    {
        GetAnimCurveDic = func1;
        GetCurveFactory = func2;
    }

    public static Dictionary<CurveFactory.CurveType, AnimationCurve> AnimCurveDic
    {
        get { return GetAnimCurveDic?.Invoke(); }
    }

    public static CurveFactory CurveFactory
    {
        get { return GetCurveFactory?.Invoke(); }
    }
}
