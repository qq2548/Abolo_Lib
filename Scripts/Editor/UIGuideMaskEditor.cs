using UnityEditor;

namespace AboloLib
{
    [CustomEditor(typeof(UIGuideMask) , true)]
    [CanEditMultipleObjects]
    public class UIGuideMaskEditor : UnityEditor.UI.ImageEditor
    {
        public override void OnInspectorGUI()
        {
            UIGuideMask myTarget = (UIGuideMask)target;
            if(myTarget != null) base.OnInspectorGUI();
            serializedObject.Update();
        }
    }
}
