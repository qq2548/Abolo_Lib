using UnityEngine;
using UnityEngine.UI;

namespace AboloLib
{
    public class RecalculateUV : MonoBehaviour
    {
        [Header("避免图集导致uv出错，需要把uv从新归一化")]
        public RendererType rendererType;
        void Start()
        {
            if (rendererType == RendererType.Image)
            {
                if (TryGetComponent(out Image image))
                {
                    //实例化Image材质，避免共用材质球出现uv错误参数
                    Material _material = new Material(image.material);
                    SetMaterial(image.sprite, _material);
                    image.material = _material;
                }
            }

            if (rendererType == RendererType.SpriteRenderer)
            {
                if (TryGetComponent(out SpriteRenderer spriteRenderer))
                {
                    SetMaterial(spriteRenderer.sprite, spriteRenderer.sharedMaterial);
                }
            }

        }

        void SetMaterial(Sprite sprite,Material material)
        {
            Vector4 UVRect = UnityEngine.Sprites.DataUtility.GetOuterUV(sprite);
            Rect originRect = sprite.rect;
            Rect textureRect = sprite.textureRect;
            float scaleX = textureRect.width / originRect.width;
            float scaleY = textureRect.height / originRect.height;
            material.SetVector("_UVRect", UVRect);
            material.SetVector("_UVScale", new Vector4(scaleX, scaleY, 0, 0));
        }
        
    }
}


