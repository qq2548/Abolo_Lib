﻿
using TMPro;

using UnityEngine;


namespace AboloLib
{
    ///B站抄来的脚本,小改了一下效果
    public class TMP_TextPopEffect : MonoBehaviour
    {
        /// <summary>
        /// 旋转角度乘数
        /// </summary>
        public float AngleMultiplier = 5f;

        /// <summary>
        /// 偏移乘数
        /// </summary>
        public float OffsetMultiplier = 10f;

        /// <summary>
        /// 动画速度
        /// </summary>
        public float Speed = 15f;

        /// <summary>
        /// 动画中显示多少个字符
        /// </summary>
        public int ShowCount = 5;

        /// <summary>
        /// 当前动画进度
        /// </summary>
        public float Schedule;

        /// <summary>
        /// 是否改变颜色
        /// </summary>
        public bool UseColor;

        /// <summary>
        /// 渐变
        /// </summary>
        public Gradient gradient;

        /// <summary>
        /// 文本组件
        /// </summary>
        private TMP_Text m_TextComponent;

        private void Awake()
        {
            m_TextComponent = GetComponent<TMP_Text>();
            Schedule = 0.0f;
        }

        private void OnEnable()
        {
            Schedule = 0.0f;
            TextAnima();
        }

        private void Update()
        {
            if (Schedule< int.MaxValue)
            {
                TextAnima();
                Schedule += Time.deltaTime * Speed;
            }
        }

        private void TextAnima()
        {
            if (m_TextComponent == null)
            {
                m_TextComponent = GetComponent<TMP_Text>();
            }

            //刷新
            m_TextComponent.ForceMeshUpdate();
            //文本组件的信息
            TMP_TextInfo textInfo = m_TextComponent.textInfo;
            if (textInfo == null)
            {
                return;
            }
            else
            {
                if (textInfo.meshInfo.Length == 0)
                {
                    return;
                }
                else
                {
                    if (textInfo.meshInfo[0].vertices == null)
                    {
                        return;
                    }
                }
            }
            //用来做旋转和缩放的矩阵
            Matrix4x4 matrix;
            //复制一下顶点数据
            TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
            //获取字符数量
            int characterCount = textInfo.characterCount;
            //文本组件没字符就不管
            if (characterCount == 0)
            {
                return;
            }

            for (int i = 0; i < characterCount; i++)
            {
                //根据动画进度计每个字符的偏移量,为0则不进行动画
                float offset = (i - Schedule + ShowCount);
                offset = Mathf.Clamp(offset, 0, offset);

                //不在屏幕内的文字不管
                if (!textInfo.characterInfo[i].isVisible) continue;

                //获取一下字符的材质下标和第一个顶点的下标------->这里为什么要取材质下标没看懂
                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                //先获取一下顶点坐标
                Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;
                Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;

                //获取左下和右下顶点坐标,然后相加除2获得字符所在中心坐标,矩阵计算要用
                Vector3 matrixOffset = (sourceVertices[vertexIndex] + sourceVertices[vertexIndex + 3]) / 2;

                //获取颜色数组
                Color32[] newVertexColors = null;
                if (UseColor) newVertexColors = textInfo.meshInfo[materialIndex].colors32;

                Vector3 jitterOffset =  offset * OffsetMultiplier * Vector3.up ;
                //与位置偏移类似,不过是反着的,在显示范围内,offset越大rota越小,让字符看起来由小变大
                var rota = (ShowCount - offset) / ShowCount;
                rota = Mathf.Clamp(rota, 0, 1);
                if (CurveAdapter.AnimCurveDic == null || CurveAdapter.AnimCurveDic.Count < 1)
                {
                    return;
                }
                float t = CurveAdapter.AnimCurveDic[CurveFactory.CurveType.TextPopScale].Evaluate(rota);
                //创建矩阵,传入位移旋转缩放
                matrix = Matrix4x4.TRS(jitterOffset, Quaternion.Euler(0, 0, offset * AngleMultiplier), Vector3.one * t);
                //每个字符由四个顶点组成,所以四个一批进行动画
                for (int index = 0; index < 4; index++)
                {
                    //超过最大显示数量的数字则不显示,将所有顶点置于原点,属实偷懒了,不过也懒得弄其他方法了,凑合用吧
                    if (i > Schedule + ShowCount)
                    {
                        destinationVertices[vertexIndex + index] = Vector3.zero;
                    }
                    else
                    {
                        //根据动画进度显示不同的颜色
                        if (UseColor) newVertexColors[vertexIndex + index] = gradient.Evaluate(rota);
                        //先减去中心点坐标,再进行旋转,差不多就是把一个矩形先移动到中心然后再旋转
                        destinationVertices[vertexIndex + index] = sourceVertices[vertexIndex + index] - matrixOffset;
                        destinationVertices[vertexIndex + index] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + index]);
                        //矩阵计算完了再把开始减的加回去
                        destinationVertices[vertexIndex + index] += matrixOffset;
                    }
                }
            }
            //把计算完的坐标颜色啥的赋值应用一下就完事了
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                m_TextComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
            if (UseColor) m_TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
    }
}