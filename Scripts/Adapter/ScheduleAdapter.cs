using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AboloLib
{
    public class ScheduleAdapter
    {
        private static Func<MonoBehaviour> schedual;
        public static void Init(Func<MonoBehaviour> func)
        {
            schedual =  func;
        }

        public static MonoBehaviour Schedual
        {
            get => schedual?.Invoke();
        }

        public static Coroutine DoSchedual(float delay , Action task)
        {
            return Schedual.StartCoroutine(ArtAnimation.ArtAnimDelayCoroutine(delay, task));
        }

        public static void DoSequenceScheduals(List<IEnumerator> enums , Action callback)
        {
            Schedual.StartCoroutine(ArtAnimation.DoSequenceActions(enums , callback));
        }
    }
}
