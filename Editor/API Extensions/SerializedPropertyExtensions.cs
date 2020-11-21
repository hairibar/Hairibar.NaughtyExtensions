using UnityEditor;
using UnityEngine;

namespace Hairibar.NaughtyExtensions.Editor
{
    public static class SerializedPropertyExtensions
    {
        public static GUIContent GetLabelContent(this SerializedProperty serializedProperty)
        {
            return new GUIContent(serializedProperty.displayName, serializedProperty.tooltip);
        }
    }
}
