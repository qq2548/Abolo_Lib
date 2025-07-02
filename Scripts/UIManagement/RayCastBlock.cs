using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AboloLib;

namespace AboloLib
{
    public class RayCastBlock : AboloSingleton<RayCastBlock>
    {
        Transform root;
        private void Awake()
        {
            base.Init();
            root = transform.Find("root");
            SetRayCastBlock(false);
        }

        public void SetRayCastBlock(bool canBlock)
        {
            root.gameObject.SetActive(canBlock);
        }
    }
}
