using System;
using UnityEngine;

namespace AboloLib
{
    public class AboloSingleton<T> : MonoBehaviour where T : class
    {
        public static T _instance;
        public virtual void Init()
        {
            _instance = this as T;
        }


#if _ARTEST_PRESENTATION
        protected void Test()
        {
            print($"My Type is {_instance.GetType()}!!");
        }
#endif
    }
}