using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AboloLib
{
    public class CurrencyCollectManager : MonoBehaviour
    {
        public static CurrencyCollectManager instance;

        public CurrencyFactory currencyFactory;
        public  UICurrencyCollect CollectSpawmerObj;
        public static UICurrencyCollect CollectSpawmer;
        private void Awake()
        {
            instance = this;
            if (CollectSpawmer == null)
            {
                CollectSpawmer = Instantiate(CollectSpawmerObj);
            }
        }

        public static void ShootFlyItems(int id , int count , Transform from , Vector3 to , float range = 1.0f)
        {
            //Debug.Log("Shoot some");
            CollectSpawmer.BoombToCollectCurrency(id, count, from, to, range);
        }

    }

}