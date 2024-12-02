using System;
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
    }
}
