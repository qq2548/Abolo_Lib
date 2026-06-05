using UnityEditor;
using UnityEditor.Playables;
using UnityEngine;

namespace AboloLib
{
    [CustomEditor(typeof(GridSystem))]
    public class GridSystemEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GridSystem gridSystem = (GridSystem)target;
            if (GUILayout.Button("生成网格"))
            {
                ArtWorkSpaceEditor.FunctionEditorOnly(() =>
                {
                    gridSystem.CreateEditorCells();
                    EditorUtility.SetDirty(gridSystem.gameObject);
                });
            }

        }
    }
}
