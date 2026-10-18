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

    public static void MoveCamera(Vector3 direction, float offset, 
        float duration, float cameraSize, AnimationCurve curve = null ,Action callback = null)
    {
        var cam = CurrentCamera.transform;
        Vector3 from = cam.position;
        Vector3 to = from + direction * offset;
        cam.MoveTo(from , to , duration , curve, callback);
        float startSize = CurrentCamera.orthographicSize;
        ScheduleAdapter.DoAnimation(duration , (value) =>
        {
            float t = curve != null?curve.Evaluate(value) : value;
            CurrentCamera.orthographicSize = Mathf.Lerp(startSize , cameraSize , value);
        });
    }

    public static void MoveCamera(Vector3 to, float duration, float cameraSize, AnimationCurve curve = null ,Action callback = null)
    {
        var cam = CurrentCamera.transform;
        Vector3 from = cam.position;
        cam.MoveTo(from , new Vector3(to.x , to.y , from.z) , duration , curve, callback);
        float startSize = CurrentCamera.orthographicSize;
        ScheduleAdapter.DoAnimation(duration , (value) =>
        {
            float t = curve != null?curve.Evaluate(value) : value;
            CurrentCamera.orthographicSize = Mathf.Lerp(startSize , cameraSize , value);
        });
    }

    public static void ResetCamera(float duration, AnimationCurve curve = null ,Action callback = null)
    {
        var cam = CurrentCamera.transform;
        Vector3 from = cam.position;
        Vector3 to = FlowerGameManager._instance.transform.TransformPoint(Vector3.zero);
        cam.MoveTo(from , new Vector3(to.x , to.y , from.z) , duration , curve, callback);
        float startSize = CurrentCamera.orthographicSize;
        ScheduleAdapter.DoAnimation(duration , (value) =>
        {
            float t = curve != null?curve.Evaluate(value) : value;
            CurrentCamera.orthographicSize = Mathf.Lerp(startSize , 5f , value);
        });
    }
}
