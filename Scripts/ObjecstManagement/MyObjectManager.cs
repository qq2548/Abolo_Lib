using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AboloLib
{
    public class MyObjectManager : MonoBehaviour
    {
        public static MyObjectManager instance;

        public MyObjectFactory objectFactory;
        public MyObjectSpawner objcectSpawmer;

        private void Start()
        {
            instance = this;
        }

    }

}