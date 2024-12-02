using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AboloLib
{
    [CreateAssetMenu(menuName = "ArtUtils/ObjectFactory")]
    public class MyObjectFactory : ScriptableObject
    {
        //[SerializeField]
        public MyObject[] prefabs;

        [SerializeField]
        private bool recycle;

        private Scene poolScene;

        private List<MyObject>[] pools;

        public int TotalCountInPool
        {
            get
            {
                int count = 0;
                for (int i = 0; i < pools.Length; i++)
                {
                    count += pools[i].Count;
                }
                return count;
            }
        }
        public MyObject Get(int objectId = 0)
        {
            MyObject instance;
            if (recycle)
            {
                if (pools == null)
                {
                    CreatePools();
                }
                List<MyObject> pool = pools[objectId];
                int lastIndex = pool.Count - 1;
                if (lastIndex >= 0)
                {
                    instance = pool[lastIndex];
                    instance.gameObject.SetActive(true);
                    pool.RemoveAt(lastIndex);
                }
                else
                {
                    instance = Instantiate(prefabs[objectId]);
                    instance.Id = objectId;
                }

                SceneManager.MoveGameObjectToScene(instance.gameObject, poolScene);
            }
            else
            {
                instance = Instantiate(prefabs[objectId]);
                instance.Id = objectId;
            }
            
            return instance;
        }

        public MyObject GetRandom()
        {
            return Get
                (
                    UnityEngine.Random.Range(0, 2)
                );
        }

        private void CreatePools()
        {
            pools = new List<MyObject>[prefabs.Length];
            for (int i = 0; i < pools.Length; i++)
            {
                pools[i] = new List<MyObject>();
            }
            if (Application.isEditor)
            {
                poolScene = SceneManager.GetSceneByName(name);
                if (poolScene.isLoaded)
                {
                    GameObject[] rootObjects = poolScene.GetRootGameObjects();
                    for (int i = 0; i < rootObjects.Length; i++)
                    {
                        MyObject pooledShape = rootObjects[i].GetComponent<MyObject>();
                        if (!pooledShape.gameObject.activeSelf)
                        {
                            pools[pooledShape.Id].Add(pooledShape);
                        }
                    }
                    return;
                }
            }
            poolScene = SceneManager.CreateScene(name);
        }

        public void Reclaim(MyObject objectToRecycle, bool hasAnimation = false)
        {
            if (recycle)
            {
                ObjectToPool(objectToRecycle);
            }
            else
            {
                ObjectToDestroy(objectToRecycle);
            }
        }

        private void ObjectToPool(MyObject objectToRecycle)
        {
            if (pools == null)
            {
                CreatePools();
            }
            pools[objectToRecycle.Id].Add(objectToRecycle);
            objectToRecycle.gameObject.SetActive(false);
        }

        private void ObjectToDestroy(MyObject objectToRecycle)
        {
            Destroy(objectToRecycle.gameObject);
        }
    }
}
