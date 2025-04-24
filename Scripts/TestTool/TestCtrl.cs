using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AboloLib
{
    public class TestCtrl : MonoBehaviour
    {
        Transform root ;
        // Start is called before the first frame update
        void Start()
        {
            root = transform.Find("root");

            var source = transform.Find("source");
            for (int i = 0; i < 12; i++)
            {
                var obj = Instantiate(source.gameObject, root);
                obj.name = i.ToString();
                TestFuc(i);
            }

            //StartCoroutine(Generation());
           //root.GetChild(5).SetSiblingIndex(99);
        }
        Vector2Int[] arr = { new Vector2Int(0 , 0), 
                                      new Vector2Int(1 ,2), 
                                      new Vector2Int(2 , 5), 
                                      new Vector2Int(3 , 6), 
                                      new Vector2Int(4 , 7), 
                                      new Vector2Int(5,10), 
                                      new Vector2Int(6, 9), 
                                      new Vector2Int(7 , 8), 
                                      new Vector2Int(8 , 1), 
                                      new Vector2Int(9 , 3), 
                                      new Vector2Int(10 , 4),
                                      new Vector2Int(11 , 11)};
        public void TestFuc(int order)
        {
            for (int i = 0; i < order+1; i++)
            {
                int num = arr.FirstOrDefault((element) => element.y == i).x;
                root.Find(i.ToString()).SetSiblingIndex(num);
            }
        }

        IEnumerator Generation()
        {
            var source = transform.Find("source");
            yield return new WaitForSeconds(1.0f);
            
            for (int i = 0; i < 12; i++)
            {
                var obj = Instantiate(source.gameObject, root);
                obj.name = i.ToString();
                TestFuc(i);
                yield return new WaitForSeconds(1.0f);
            }
        }
    }
}
