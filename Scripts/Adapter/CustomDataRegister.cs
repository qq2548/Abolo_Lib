using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AboloLib
{
    public static class CustomDataRegister
    {
        public static Func<string> getNamespace;
        public static void Init(Func<string> func)
        {
            getNamespace = func;
        }

        public static string CustomNamespace
        {
            get => getNamespace?.Invoke();
        }
    }
}
