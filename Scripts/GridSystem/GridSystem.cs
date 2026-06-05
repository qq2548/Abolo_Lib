using System;
using System.Collections;
using System.Collections.Generic;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using UnityEditor.SceneManagement;
using UnityEngine;
using static UnityEngine.Mathf;

namespace AboloLib
{
    public class CellData
    {
        public GameObject cellObject;
        public Action OnStateChanged = null;
        private int state;
        public int State
        {
            get => state;
            set
            {
                 state = value;
                OnStateChanged?.Invoke();
            }
        }
    }
    public class GridSystem : MonoBehaviour
    {
        [SerializeField]
        private Grid grid;
        public Grid MyGrid
        {
            get => grid;
            set => grid = value;
        }
        Transform cellsRoot;
        [SerializeField] Vector2Int _gridSize;
        [SerializeField] Vector2 _cellSize;
        [SerializeField] GameObject[] _cellScources;

        public void Setup()
        {
            cellsRoot = transform.Find("cells");
            ArtUtility.ClearChildGameObjects(cellsRoot);
            Vector3 startPos = transform.Find("start").position;
            MyGrid = CreateGrid(startPos , _gridSize , _cellSize);
            var objects = CreateGridObjects(MyGrid , _cellScources , cellsRoot);
            foreach (var cell in MyGrid.Cells)
            {
                cell.Value.cellObject = objects[cell.Key];
            }
        }

        public static Grid CreateGrid(Vector3 startPos , Vector2Int gridSize , Vector2 cellSize)
        {
            Grid grid = new Grid(startPos , gridSize , cellSize);
            grid.Cells.Clear();
            for (int a = 0; a < gridSize.y; a++)
            {
                for (int b = 0; b < gridSize.x; b++)
                {
                    grid.Cells.Add(new Vector2Int(b, a) , new CellData());
                }
            }
            return grid;
        }

        public static Dictionary<Vector2Int , GameObject> CreateGridObjects(Grid grid,GameObject[] sourceObj, Transform parent = null, bool isOnGUI = false, bool startFromUpleft = false)
        {
            Dictionary<Vector2Int, GameObject> objects = new Dictionary<Vector2Int, GameObject>();
            objects.Clear();
            int index = 0;
            foreach (var item in grid.Cells)
            {
                Vector2Int coord = item.Key;
                Vector3 position = GetPositionByCoord(grid, coord, isOnGUI , startFromUpleft);
                int num = index % sourceObj.Length;
                string name = sourceObj[num].name;
                name = index.ToString() + "_" + name + "_" + coord.ToString();
                GameObject obj = Instantiate(sourceObj[num], parent);
                obj.name = name;
                float scalor = 1.0f;
                if (isOnGUI) scalor = ArtUtility.UISpaceWorldPositionScalor(UICanvasAdapter.CurrentCanvas);
                obj.transform.position = position + new Vector3(grid.CellSize.x * 0.5f * scalor, grid.CellSize.y * 0.5f * scalor * (startFromUpleft? -1.0f : 1.0f), 0.0f);
                obj.transform.localScale = grid.CellSize;
                objects.Add(coord, obj);
                index++;
            }
            return objects;
        }

        public static Dictionary<Vector2Int, GameObject> CreateGridObjectsUpLeft(Grid grid, GameObject[] sourceObj, Transform parent = null, bool isOnGUI = false , bool startFromUpleft = false)
        {
            Dictionary<Vector2Int, GameObject> objects = new Dictionary<Vector2Int, GameObject>();
            objects.Clear();
            int index = 0;
            foreach (var item in grid.Cells)
            {
                Vector2Int coord = item.Key;
                Vector3 position = GetPositionByCoord(grid, coord, isOnGUI , startFromUpleft);
                int num = index % sourceObj.Length;
                string name = sourceObj[num].name;
                name = index.ToString() + "_" + name + "_" + coord.ToString();
                GameObject obj = Instantiate(sourceObj[num], parent);
                obj.name = name;
                float scalor = 1.0f;
                if (isOnGUI) scalor = ArtUtility.UISpaceWorldPositionScalor(UICanvasAdapter.CurrentCanvas);
                Debug.Log(scalor);
                obj.transform.position = position + new Vector3(grid.CellSize.x * 0.5f * scalor, -grid.CellSize.y * 0.5f * scalor, 0.0f);
                obj.transform.localScale = grid.CellSize;
                objects.Add(coord, obj);
                index++;
            }
            return objects;
        }


        public static Vector2Int GetGridCoord(Grid grid, Vector3 mouseWorldPos)
        {
            Vector2Int coord = new Vector2Int();
            Vector3 localMousePos = mouseWorldPos - grid.StartPosition;
            coord.x = FloorToInt(localMousePos.x / grid.CellSize.x);
            coord.y = FloorToInt(localMousePos.y / grid.CellSize.y);
            return coord;
        }

        public static Vector3 GetPositionByCoord(Grid grid , Vector2Int coord , bool isOnGUI = false , bool startFromUpleft = false)
        {
            float factor = 1.0f;
            if (isOnGUI)
            {
                factor = ArtUtility.UISpaceWorldPositionScalor(UICanvasAdapter.CurrentCanvas);
            }
            Vector3 position = Vector3.zero;
            position.x = grid.StartPosition.x + coord.x * grid.CellSize.x * factor;
            position.y = grid.StartPosition.y + coord.y * grid.CellSize.y * factor * (startFromUpleft? -1f : 1f);

            if (!grid.Cells.ContainsKey(coord))
            {
                Debug.LogWarning($"this grid do not contains coord {coord}! this position is outside grid!");
            }

            return position;
        }

        public static Vector3 GetSnappingPosition(Grid grid , Vector2Int size , Vector3 mouseWorldPosition)
        {
            Vector3 result;
            Vector2Int pivot = new Vector2Int(FloorToInt(size.x * 0.5f) , FloorToInt(size.y * 0.5f));
            Vector2Int downLeft = new Vector2Int(FloorToInt(size.x * 0.5f) , FloorToInt(size.y * 0.5f));
            Vector2Int upperRight = new Vector2Int(CeilToInt(size.x * 0.5f)-1 , CeilToInt(size.y * 0.5f)-1);
            Vector3 offset = new Vector3((size.x) * 0.5f%1 * grid.CellSize.x, (size.y) * 0.5f%1* grid.CellSize.y , 0.0f);
            Vector2Int coord = GetGridCoord(grid , mouseWorldPosition);

            Vector2Int snap_coord = new Vector2Int(coord.x , coord.y);
            snap_coord = ConstraintCoordInGrid(grid , snap_coord , downLeft , upperRight);
            Debug.Log($"coord is {coord}---------pivot is {pivot}------snap_coord is {snap_coord}");
            result = GetPositionByCoord(grid , snap_coord);
            return result + offset;
        }
        public static Vector2Int ConstraintCoordInGrid(Grid grid , Vector2Int coord , Vector2Int downLeft , Vector2Int upperRight)
        {
            Vector2Int result = coord;
            if(coord.x < downLeft.x) result.x = downLeft.x;
            if(coord.x >= grid.Size.x - upperRight.x) result.x = grid.Size.x -1- upperRight.x;
            if(coord.y < downLeft.y) result.y = downLeft.y;
            if(coord.y >= grid.Size.y-upperRight.y) result.y = grid.Size.y -1-upperRight.y;
            return result;
        }

        /// <summary>
        /// 菱形向量数列
        /// </summary>
        public static readonly Vector2Int[] RhombusDirections =
        {
                new Vector2Int(-1 , 1),
                new Vector2Int(1 , 1),
                new Vector2Int(1 , -1),
                new Vector2Int(-1 ,-1),
        };
        /// <summary>
        /// 获取斜方范围网格坐标
        /// </summary>
        /// <param name="coord">中心坐标</param>
        /// <param name="distance">距离</param>
        /// <returns></returns>
        public static List<Vector2Int> GetSurroundCroodsByRhombus(Vector2Int coord, int distance)
        {
            if (distance < 0)
            {
                return null;
            }
            List<Vector2Int> neighbors = new List<Vector2Int>();
            Vector2Int currVec = new Vector2Int(coord.x, coord.y - distance);
            neighbors.Add(currVec);
            //distance - 1 避免重复添加起点
            for (int i = 0; i < 4 * distance - 1; i++)
            {
                Vector2Int curDirr = RhombusDirections[i / distance];//更新方向
                currVec = currVec + curDirr;//填入数值
                neighbors.Add(currVec);
            }
            Vector2Int v = neighbors[0];
            neighbors.RemoveAt(0);
            neighbors.Add(v);
            return neighbors;
        }
        
        /// <summary>
        /// 方形向量数列
        /// </summary>
        public static readonly Vector2Int[] SquareDirections =
        {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left
        };
        /// <summary>
        /// 获取范围最外圈的所有网格
        /// </summary>
        /// <param name="coord">中心坐标</param>
        /// <param name="distance">距离</param>
        /// <returns></returns>
        public static List<Vector2Int> GetSurroundCoordsBySquare(Vector2Int coord, int distance)
        {
            if (distance < 0)
            {
                return null;
            }
            List<Vector2Int> neighbors = new List<Vector2Int>();
            Vector2Int currVec = new Vector2Int(coord.x - distance, coord.y - distance);
            neighbors.Add(currVec);
            //distance - 1 避免重复添加起点
            for (int i = 0; i < 8 * distance - 1; i++)
            {
                Vector2Int curDirr = SquareDirections[i / (distance * 2)];//更新方向
                currVec = currVec + curDirr;//填入数值
                neighbors.Add(currVec);
            }
            Vector2Int v = neighbors[0];
            neighbors.RemoveAt(0);
            neighbors.Add(v);
            return neighbors;
        }

        public static List<Vector2Int> GetHorizontalNeighbors(Vector2Int coord, int distance , bool includeSelf = false)
        {
            if (distance < 0)
            {
                return null;
            }
            List<Vector2Int> neighbors = new List<Vector2Int>();
            neighbors.Clear();
            if (includeSelf)
            {
                neighbors.Add(coord);
            }
            for (int i = 0; i < distance; i++)
            {
                neighbors.Add(coord + Vector2Int.right * (i+1));
                neighbors.Add(coord + Vector2Int.left * (i+1));
            }
            return neighbors;
        }

        public static List<Vector2Int> GetCrosslNeighbors(Vector2Int coord, bool includeSelf = false)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>();
            neighbors.Clear();
            if (includeSelf)
            {
                neighbors.Add(coord);
            }
            for (int i = 0; i < SquareDirections.Length; i++)
            {
                neighbors.Add(coord + SquareDirections[i] * (i + 1));
            }
            return neighbors;
        }

        public static List<Vector2Int> GetSlopeCrosslNeighbors(Vector2Int coord, bool includeSelf = false)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>();
            neighbors.Clear();
            if (includeSelf)
            {
                neighbors.Add(coord);
            }
            for (int i = 0; i < RhombusDirections.Length; i++)
            {
                neighbors.Add(coord + SquareDirections[i] * (i + 1));
            }
            return neighbors;
        }

        /// <summary>
        /// 2D空间下对网格坐标向量进行旋转
        /// </summary>
        /// <param name="oriVec">需要旋转的向量</param>
        /// <param name="redius">弧度表示的角度值，范围-PI ~ PI</param>
        /// <returns></returns>
        public static Vector2Int VectorRotate2D(Vector2Int oriVec, float redius)
        {
            int x = RoundToInt(oriVec.x * Cos(redius) + oriVec.y * Sin(redius));
            int y = RoundToInt(oriVec.y * Cos(redius) - oriVec.x * Sin(redius));
            return new Vector2Int(x, y);
        }

        public static Vector3 GetTargetDirectionPostion(Grid grid ,Vector2Int fromCoord , Vector2Int direction , float distance)
        {
            Vector3 from = GetPositionByCoord(grid , fromCoord);
            Vector3 normalizedDir = new Vector3(direction.x / direction.magnitude, direction.y / direction.magnitude , 0.0f);
            Vector3 to = from + normalizedDir * distance;
            return to;
        }

        public void CellSwing(Transform cellTran , Vector2Int startCoord , Vector2Int direction , float duration ,float length)
        {
            Vector3 from = cellTran.position;
            Vector3 to = GetTargetDirectionPostion(grid , startCoord , direction , length);
            Action<float> _deltaAnim = (value) =>
            {
                cellTran.position = Vector3.Lerp(from , to , CurveAdapter.AnimCurveDic[CurveFactory.CurveType.Spring].Evaluate(value));
            };

            StartCoroutine(ArtAnimation.DoAnimation(1.0f , _deltaAnim));
        }

#region 编辑器方法
    #if UNITY_EDITOR
        public void CreateEditorCells()
        {
            cellsRoot = transform.Find("cells");
            ArtUtility.ClearChildGameObjects(cellsRoot);
            Vector3 startPos = transform.Find("start").position;
            var grid = CreateGrid(startPos , _gridSize , _cellSize);
            CreateGridObjects(grid , _cellScources , cellsRoot);
        }
    #endif
#endregion

    }
}
