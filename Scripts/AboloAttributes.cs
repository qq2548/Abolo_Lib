using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AboloLib
{
    public class AboloAttributes
    {

    }

    [AttributeUsage(AttributeTargets.Field)]
    public class PrefabSelectAttribute : PropertyAttribute
    {
        public string AssetPath;
        public PrefabSelectAttribute(string path)
        {
            AssetPath = path;
        }
    }
}
