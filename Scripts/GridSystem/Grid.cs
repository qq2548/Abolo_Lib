using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AboloLib
{
    public class Grid
    {
        private Vector3 startPos;
        /// <summary>
        /// 网格起始坐标，只读
        /// </summary>
        public Vector3 StartPosition
        {
            get => startPos;
        }
        private int width;
        private int height;
        /// <summary>
        /// grid size ,只读
        /// </summary>
        public Vector2Int Size
        {
            get => new Vector2Int(width, height);
        }
        private Vector2 cellSize;
        /// <summary>
        /// 网格大小 ,只读
        /// </summary>
        public Vector2 CellSize
        {
            get => cellSize;
        }
        //private List<Vector2Int> coordList;
        ///// <summary>
        ///// 网格坐标数组,只读
        ///// </summary>
        //public List<Vector2Int> CoordList
        //{
        //    get => coordList;
        //}

        private Dictionary<Vector2Int, CellData> cells;
        public Dictionary<Vector2Int, CellData> Cells
        {
            get => cells;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="gridSize">网格宽高</param>
        /// <param name="cellSize">单元网格尺寸</param>
        /// <param name="coordList">网格坐标数组</param>
        public Grid(Vector3 startPos , Vector2Int gridSize , Vector2 cellSize)
        {
            this.startPos = startPos;
            this.width = gridSize.x;
            this.height = gridSize.y;
            this.cellSize = cellSize;
            //this.coordList =new List<Vector2Int>();
            this.cells = new Dictionary<Vector2Int , CellData>();
        }
    }

    [System.Serializable]
    public class GridData
    {
        [SerializeField] Vector2Int gridSize;
        public Vector2Int Size
        {
            get => gridSize;
        }
        [SerializeField] Vector2 cellSize;
        public Vector2 CellSize
        {
            get => cellSize;
        }
        [SerializeField] GameObject[] cellSource;
        public GameObject[] CellSource
        {
            get => cellSource;
        }
        [SerializeField] Transform gridStartRoot;
        public Transform StartRoot
        {
            get => gridStartRoot;
        }
        [SerializeField] Transform gridObjectRoot;
        public Transform ObjectRoot
        {
            get => gridObjectRoot;
        }
    }
}
