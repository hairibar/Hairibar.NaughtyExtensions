# New drawers
Generally speaking, drawers must have these overloads:

## Auto-layout
- `public static void Draw_Layout(SerializedProperty serializedProperty, ...);`   
  - Calls `Draw_Layout(serializedProperty, ..., serializedProperty.GetLabelContent())`

- `public static void Draw_Layout(SerializedProperty serializedProperty, ..., GUIContent label);`
  - ``` csharp
      //Example implementation
      Rect rect = EditorGUILayout.GetControlRect();
      EditorGUI.BeginProperty(rect, GUIContent.none, serializedProperty);

      serializedProperty.floatValue = Draw(rect, guiContent, serializedProperty.floatValue, ...);

      EditorGUI.EndProperty();
      ```
- `public static void Draw_Layout(GUIContent label, T value, ...);`
  - This version does not use a SerializedProperty. Some attributes make no sense without a SerializedProperty. In such a case, skip this overload.
  - ``` csharp
    //Example implementation
    Rect rect = EditorGUILayout.GetControlRect();
    return Draw(EditorGUILayour.GetControlRect(), guiContent, value, ...);
    ```

## Manual layout
- `public static void Draw(Rect rect, SerializedProperty serializedProperty, ...);`
- `public static void Draw(Rect rect, SerializedProperty serializedProperty, ..., GUIContent label)`
  - ``` csharp
      //Example implementation
      EditorGUI.BeginProperty(rect, GUIContent.none, serializedProperty);

      serializedProperty.floatValue = Draw(rect, guiContent, serializedProperty.floatValue, ...);

      EditorGUI.EndProperty();
      ```
- `public static T Draw(Rect rect, GUIContent label, T value, ...);`
  - This version implements the actual drawing logic. All other overloads eventually call this one.
  - If non-SerializedProperty overloads don't make sense, the drawing is implemented by `public static void Draw(Rect rect, SerializedProperty serializedProperty, ..., GUIContent label)`.

    