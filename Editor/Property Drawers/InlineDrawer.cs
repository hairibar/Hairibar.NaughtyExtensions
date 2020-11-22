using NaughtyAttributes.Editor;
using UnityEditor;
using UnityEngine;

namespace Hairibar.NaughtyExtensions
{
    [CustomPropertyDrawer(typeof(InlineAttribute))]
    public class InlineDrawer : PropertyDrawerBase
    {
        protected override void OnGUI_Internal(Rect rect, SerializedProperty property, GUIContent label)
        {
            if (property.hasChildren)
            {
                InlineAttribute attribute = PropertyUtility.GetAttribute<InlineAttribute>(property);

                if (attribute.ShowHeaderAndBox)
                {
                    rect.position += new Vector2(0, EditorGUIUtility.singleLineHeight * 0.2f);

                    EditorGUI.LabelField(rect, label, EditorStyles.boldLabel);
                    NaughtyEditorGUI.BeginBoxGroup_Layout();
                }

                //Move to first child
                SerializedProperty child = property.Copy();
                child.Next(true);

                SerializedProperty endProperty = property.GetEndProperty(true);

                bool enterChildren;
                if (attribute.ShowHeaderAndBox)
                {
                    enterChildren = EditorGUILayout.PropertyField(child);
                }
                else
                {
                    enterChildren = EditorGUI.PropertyField(rect, child);
                }

                while (child.Next(enterChildren) && !SerializedProperty.EqualContents(child, endProperty))
                {
                    enterChildren = EditorGUILayout.PropertyField(child);
                }

                if (attribute.ShowHeaderAndBox)
                {
                    NaughtyEditorGUI.EndBoxGroup_Layout();
                }
            }
            else
            {
                EditorGUILayout.PropertyField(property);
                NaughtyEditorGUI.HelpBox_Layout("Can't use [Inline] on a property with no children.", MessageType.Warning);
            }
        }
    }
}
