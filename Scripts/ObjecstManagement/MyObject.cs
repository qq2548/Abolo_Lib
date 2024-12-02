using System;
using UnityEngine;

namespace AboloLib
{
    public class MyObject : MonoBehaviour
    {
        private int id = int.MinValue;
        
        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                if (id == int.MinValue && value != int.MinValue)
                {
                    id = value;
                }
                else
                {
                    Debug.Log("Not allowed to change Id!!");
                }
            }
        }

    }

}