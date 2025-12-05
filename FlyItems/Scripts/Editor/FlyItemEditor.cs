using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;


namespace AboloLib
{
    [CustomEditor(typeof(FlyItem))]
    public class FlyItemEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            FlyItem fly_item = (FlyItem)target;
            if (GUILayout.Button("载入默认设置"))
            {
                //记录修改，用于撤销误操作
                Undo.RecordObject(fly_item, "载入默认设置参数");
                var data  = AssetDatabase.LoadAssetAtPath<FlyDataPreset>("Assets/Art/Scripts/FlyItems/DefaultFlyDataPreset.asset").FlyData;

                fly_item.MyFlyData.DisableRandomShootPosition = data.DisableRandomShootPosition;
                fly_item.MyFlyData.ShootDuraiotn = data.ShootDuraiotn;
                fly_item.MyFlyData.FlyDelay = data.FlyDelay;
                fly_item.MyFlyData.FlyDuraiotn = data.FlyDuraiotn;
                fly_item.MyFlyData.Range = data.Range;
                fly_item.MyFlyData.ShootPosCurve = data.ShootPosCurve;
                fly_item.MyFlyData.ShootScaleCurve = data.ShootScaleCurve;
                fly_item.MyFlyData.FlyPosCurve = data.FlyPosCurve;
                fly_item.MyFlyData.FlyScaleCurve = data.FlyScaleCurve;
                fly_item.MyFlyData.FlyPosOffsetCurve = data.FlyPosOffsetCurve;
                fly_item.MyFlyData.MinDegree = data.MinDegree;
                fly_item.MyFlyData.MaxDegree = data.MaxDegree;
                fly_item.MyFlyData.OnDoneFx = data.OnDoneFx;

                ////用反射会直接把两个data引用关联，不是想要的结果
                //Type type = fly_item.MyFlyData.GetType();
                //var PropertyInfo = type.GetFields();

                //foreach (var info in PropertyInfo)
                //{
                //    var value = info.GetValue(data);
                //    info.SetValue(fly_item.MyFlyData , value);
                //}

                EditorUtility.SetDirty(fly_item);
                data = null;
            }
        }
    }
}
