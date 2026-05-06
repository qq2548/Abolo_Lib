using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AboloLib
{
    /// <summary>
    /// UI Image Shader 自定义编辑器面板
    /// </summary>
    public class AboloImageShaderGUI : ShaderGUI
    {
        string[] blendops = {"Normal" , "Additive" , "Multiply"};
        
        public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            Material targetMat = materialEditor.target as Material;
            EditorGUI.BeginChangeCheck();
            if(targetMat.HasProperty("_BlendMode") && targetMat.HasProperty("_BlendSrc") && targetMat.HasProperty("_BlendDst"))
            {
                int selectIndex = targetMat.GetInt("_BlendMode");
                SetMaterilBlend(selectIndex , targetMat); 
                selectIndex = EditorGUILayout.Popup("BlendMode", selectIndex , blendops);
                if (EditorGUI.EndChangeCheck())
                {   
                    SetMaterilBlend(selectIndex , targetMat); 
                    EditorUtility.SetDirty(targetMat);
                }
            }
            base.OnGUI (materialEditor, properties);  
        }

        private void SetMaterilBlend(int index , Material material)
        {
            material.SetInt("_BlendMode", index); 
            if(index == 0)
            {
                material.SetInt("_BlendSrc", 5);
                material.SetInt("_BlendDst", 10);  
            }
            if(index == 1)
            {
                material.SetInt("_BlendSrc", 5);
                material.SetInt("_BlendDst", 1);                    
            }
            if(index == 2)
            { 
                material.SetInt("_BlendSrc", 2);
                material.SetInt("_BlendDst", 0);                    
            }
        }
    }
    /// <summary>
    /// 2D Sprite Shader 自定义编辑器面板
    /// </summary>
    public class AboloSpriteShaderGUI : ShaderGUI
    {
        string[] blendops = {"Normal" , "Additive" , "Multiply"};
        
        public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            Material targetMat = materialEditor.target as Material;
            EditorGUI.BeginChangeCheck();
            if(targetMat.HasProperty("_BlendMode") && targetMat.HasProperty("_BlendSrc") && targetMat.HasProperty("_BlendDst"))
            {
                int selectIndex = targetMat.GetInt("_BlendMode");
                SetMaterilBlend(selectIndex , targetMat); 
                selectIndex = EditorGUILayout.Popup("BlendMode", selectIndex , blendops);
                if (EditorGUI.EndChangeCheck())
                {   
                    SetMaterilBlend(selectIndex , targetMat); 
                    EditorUtility.SetDirty(targetMat);
                }
            }
            base.OnGUI (materialEditor, properties);  
        }

        private void SetMaterilBlend(int index , Material material)
        {
            material.SetInt("_BlendMode", index); 
            if(index == 0)
            {
                material.SetInt("_BlendSrc", 1);
                material.SetInt("_BlendDst", 10);  
            }
            if(index == 1)
            {
                material.SetInt("_BlendSrc", 1);
                material.SetInt("_BlendDst", 1);                    
            }
            if(index == 2)
            { 
                material.SetInt("_BlendSrc", 2);
                material.SetInt("_BlendDst", 3);                    
            }
        }
    }
}
