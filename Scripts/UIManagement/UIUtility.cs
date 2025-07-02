using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static AboloLib.ArtUtility;

namespace Study_Farm
{
    public static class UIUtility
    {
       public static GameObject DuplicateItemWithNameAndCount(GameObject source , int itemId , int count  , Transform parent = null)
        {
            var pgo = DuplicateItemWithName(source , itemId , parent).transform;
            pgo.Find("count").GetComponent<TextMeshProUGUI>().text = count.ToString();
            pgo.gameObject.SetActive(true);
            return pgo.gameObject;
        }

        public static GameObject DuplicateItemWithName(GameObject source, int itemId, Transform parent = null)
        {
            var pgo = DuplicateItem(source , itemId , parent).transform;
            pgo.Find("name").GetComponent<TextMeshProUGUI>().text = GameDictionary.ProductDataDic[itemId].Name;
            pgo.gameObject.SetActive(true);
            return pgo.gameObject;
        }

        public static GameObject DuplicateItem(GameObject source, int itemId, Transform parent = null)
        {
            string resPath = "Sprites/Products/" + itemId.ToString();
            Sprite spr_instance = AboloLib.ArtUtility.InstantiateSpriteFromResource(resPath);
            Transform pgo = Object.Instantiate(source, parent).transform;
            pgo.Find("icon").GetComponent<Image>().sprite = spr_instance;
            pgo.gameObject.SetActive(true);
            return pgo.gameObject;
        }

        public static void ClearUINode(Transform node)
        {
            if (node.childCount > 0)
            {
                int index = node.childCount;
                for (int i = 0; i < index; i++)
                {
                    Object.DestroyImmediate(node.GetChild(0).gameObject);
                }
            }
        }
    }
}
