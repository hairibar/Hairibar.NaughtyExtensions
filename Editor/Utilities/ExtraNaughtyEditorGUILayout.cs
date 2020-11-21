using UnityEditor;

namespace Hairibar.NaughtyExtensions.Editor
{
    public static class ExtraNaughtyEditorGUILayout
    {
        public static void Header(string text)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
        }
    }
}
