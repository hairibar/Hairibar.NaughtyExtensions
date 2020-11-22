using NaughtyAttributes.Editor;
using UnityEditor;
using UnityEngine;

namespace Hairibar.NaughtyExtensions.Editor
{
    [CustomPropertyDrawer(typeof(EditTimeOnlyAttribute))]
    public class EditTimeOnlyDrawer : PropertyDrawerBase
    {
        protected override void OnGUI_Internal(Rect rect, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.DisabledGroupScope(disabled: Application.isPlaying))
            {
                EditorGUI.PropertyField(rect, property, label, true);
            }
        }
    }
}
