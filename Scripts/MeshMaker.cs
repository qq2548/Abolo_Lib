using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AboloLib
{
    //一堆代码错误，瞎改到不报错的，效果肯定有问题，空了研究
    public class MeshMaker : MonoBehaviour
    {
        public Sprite m_pokeSprite;
        public MeshFilter m_tempMesh;

        public void MakeMesh()
        {
            Mesh mesh = new Mesh();

            //create vertices
            List<Vector3> vertices = new List<Vector3>();
            vertices.AddRange(Array.ConvertAll(m_pokeSprite.vertices, i => (Vector3)i));
            List<Vector3> backVertices = new List<Vector3>(vertices);
            backVertices = ShiftVertices(backVertices);
            vertices.AddRange(backVertices);
            mesh.vertices = vertices.ToArray();

            //create triangles
            List<int> triangles = new List<int>();
            triangles.AddRange(Array.ConvertAll(m_pokeSprite.triangles, i => (int)i));
            List<int> backTriangles = new List<int>(triangles);
            backTriangles = ShiftAndFlipTriangleIndexes(backTriangles, backVertices.Count);
            List<int> middleTriangels = CreateMiddleTriangles(triangles, backTriangles);
            triangles.AddRange(backTriangles);
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            triangles.AddRange(middleTriangels);
            mesh.triangles = triangles.ToArray();

            //create uvs
            List<Vector2> uvs = new List<Vector2>(m_pokeSprite.uv);
            List<Vector2> newUvs = new List<Vector2>(uvs);
            uvs.AddRange(newUvs);
            mesh.uv = uvs.ToArray();

            //assign to mesh in scene and save mesh as asset
            m_tempMesh.sharedMesh = mesh;
            AssetDatabase.CreateAsset(mesh, "Assets/meshSaved.asset");
        }

        private List<Vector3> ShiftVertices(List<Vector3> verticies)
        {
            float dist = 1f;

            for (int i = 0; i < verticies.Count; i++)
            {
                verticies[i] = new Vector3(verticies[i].x, verticies[i].y, verticies[i].z + dist);
            }

            return verticies;
        }

        private List<int> ShiftAndFlipTriangleIndexes(List<int> indecies, int shiftAmount)
        {
            List<int> toReturn = new List<int>(indecies);
            for (int i = 0; i < toReturn.Count; i++)
            {
                toReturn[i] += shiftAmount;
            }

            for (int i = 0; i < toReturn.Count; i+=3)
            {
                int i0 = toReturn[i];
                int i2 = toReturn[i+2];

                toReturn[i] = i2;
                toReturn[i + 2] = i0;
            }

            return toReturn;
        }

        private List<int> CreateMiddleTriangles(List<int> frontTriangles, List<int> backTriangles)
        {
            List<int> toReturn = new List<int>();

            for (int i = 0; i < frontTriangles.Count; i += 3)
            {
            toReturn.AddRange(MakeQuad(frontTriangles[i], frontTriangles[i + 1], backTriangles[i + 2], backTriangles[i + 1]));
            toReturn.AddRange(MakeQuad(frontTriangles[i + 1], frontTriangles[i + 2], backTriangles[i + 1], backTriangles[i]));
            toReturn.AddRange(MakeQuad(frontTriangles[i + 2], frontTriangles[i + 3], backTriangles[i] , backTriangles[i + 2]));
            }

            return toReturn;
        }

        private List<int> MakeQuad(int x0, int x1, int y0, int y1)
        {
            List<int> toReturn = new List<int>();

            toReturn.Add(x0);
            toReturn.Add(y0);
            toReturn.Add(y1);

            toReturn.Add(x0);
            toReturn.Add(y1);
            toReturn.Add(x1);

            return toReturn;
        }
    }
}
