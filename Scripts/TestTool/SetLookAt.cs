using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AboloLib
{
    [ExecuteInEditMode]
    public class SetLookAt : MonoBehaviour
    {
        #region 编辑器方法
#if _ARTEST_PRESENTATION
        public void ChildrenSpriteLookAtCamera()
        {
            Transform target = GameObject.Find("LookAtPos").transform;
            ResetLookAtPos(target);
            var sprite_children = transform.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var item in sprite_children)
            {
                ArtUtility.LookAtTarget(item.transform, target);
            }
        }

        public void LookAtTarget()
        {
            Transform target = GameObject.Find("LookAtPos").transform;
            ResetLookAtPos(target);
            ArtUtility.LookAtTarget(transform, target);
        }

        void ResetLookAtPos(Transform lookAtPos)
        {
            Scene ArtScene = SceneManager.GetSceneByName("Art");
            GameObject CameraRoot = ArtScene.GetRootGameObjects().FirstOrDefault(s => s.name == "CameraRoot");
            lookAtPos.position = CameraRoot.transform.GetComponentInChildren<Camera>().transform.position;
        }

        public void IncreaseSubRendererSortingOrder()
        {
            var rs = transform.GetComponentsInChildren<Renderer>();
            foreach (var item in rs)
            {
                item.sortingOrder += 1;
            }
        }

        public void DecreaseSubRendererSortingOrder()
        {
            var rs = transform.GetComponentsInChildren<Renderer>();
            foreach (var item in rs)
            {
                item.sortingOrder -= 1;
            }
        }

#endif
        #endregion

    }

}