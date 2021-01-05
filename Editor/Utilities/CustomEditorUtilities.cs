﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

//Largely based on https://github.com/lordofduct/spacepuppy-unity-framework/blob/master/SpacepuppyBaseEditor/EditorHelper.cs
//Copyright(c) 2015, Dylan Engelman, Jupiter Lighthouse Studio
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

namespace Hairibar.NaughtyExtensions.Editor
{
    public static class CustomEditorUtilities
    {
        public const string PROP_SCRIPT = "m_Script";
        public const string PROP_ORDER = "_order";
        public const string PROP_ACTIVATEON = "_activateOn";

        public const float OBJFIELD_DOT_WIDTH = 18f;

        #region SerializedProperty Helpers
        public static SerializedProperty FindPropertyFromAutoProperty(this SerializedObject serializedObject, string autoPropertyName)
        {
            string backingFieldName = ReflectionUtilities.GetAutoBackingFieldName(autoPropertyName);
            return serializedObject.FindProperty(backingFieldName);
        }


        public static IEnumerable<SerializedProperty> GetChildren(this SerializedProperty property)
        {
            property = property.Copy();
            var nextElement = property.Copy();
            bool hasNextElement = nextElement.NextVisible(false);
            if (!hasNextElement)
            {
                nextElement = null;
            }

            property.NextVisible(true);
            while (true)
            {
                if ((SerializedProperty.EqualContents(property, nextElement)))
                {
                    yield break;
                }

                yield return property;

                bool hasNext = property.NextVisible(false);
                if (!hasNext)
                {
                    break;
                }
            }
        }

        public static System.Type GetTargetType(this SerializedObject obj)
        {
            if (obj == null) return null;

            if (obj.isEditingMultipleObjects)
            {
                var c = obj.targetObjects[0];
                return c.GetType();
            }
            else
            {
                return obj.targetObject.GetType();
            }
        }

        public static System.Type GetTargetType(this SerializedProperty prop)
        {
            if (prop == null) return null;

            System.Reflection.FieldInfo field;
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Generic:
                    return TypeUtilities.FindType(prop.type) ?? typeof(object);
                case SerializedPropertyType.Integer:
                    return prop.type == "long" ? typeof(int) : typeof(long);
                case SerializedPropertyType.Boolean:
                    return typeof(bool);
                case SerializedPropertyType.Float:
                    return prop.type == "double" ? typeof(double) : typeof(float);
                case SerializedPropertyType.String:
                    return typeof(string);
                case SerializedPropertyType.Color:
                    {
                        field = GetFieldInfo(prop);
                        return field != null ? field.FieldType : typeof(Color);
                    }
                case SerializedPropertyType.ObjectReference:
                    {
                        field = GetFieldInfo(prop);
                        return field != null ? field.FieldType : typeof(UnityEngine.Object);
                    }
                case SerializedPropertyType.LayerMask:
                    return typeof(LayerMask);
                case SerializedPropertyType.Enum:
                    {
                        field = GetFieldInfo(prop);
                        return field != null ? field.FieldType : typeof(System.Enum);
                    }
                case SerializedPropertyType.Vector2:
                    return typeof(Vector2);
                case SerializedPropertyType.Vector3:
                    return typeof(Vector3);
                case SerializedPropertyType.Vector4:
                    return typeof(Vector4);
                case SerializedPropertyType.Rect:
                    return typeof(Rect);
                case SerializedPropertyType.ArraySize:
                    return typeof(int);
                case SerializedPropertyType.Character:
                    return typeof(char);
                case SerializedPropertyType.AnimationCurve:
                    return typeof(AnimationCurve);
                case SerializedPropertyType.Bounds:
                    return typeof(Bounds);
                case SerializedPropertyType.Gradient:
                    return typeof(Gradient);
                case SerializedPropertyType.Quaternion:
                    return typeof(Quaternion);
                case SerializedPropertyType.ExposedReference:
                    {
                        field = GetFieldInfo(prop);
                        return field != null ? field.FieldType : typeof(UnityEngine.Object);
                    }
                case SerializedPropertyType.FixedBufferSize:
                    return typeof(int);
                case SerializedPropertyType.Vector2Int:
                    return typeof(Vector2Int);
                case SerializedPropertyType.Vector3Int:
                    return typeof(Vector3Int);
                case SerializedPropertyType.RectInt:
                    return typeof(RectInt);
                case SerializedPropertyType.BoundsInt:
                    return typeof(BoundsInt);
                default:
                    {
                        field = GetFieldInfo(prop);
                        return field != null ? field.FieldType : typeof(object);
                    }
            }
        }

        /// <summary>
        /// Gets the object the property represents.
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static object GetTargetObject(this SerializedProperty prop)
        {
            if (prop == null) return null;

            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }
        /// <summary>
        /// Gets the object that the property is a member of
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static object GetTargetObjectOwner(this SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements.Take(elements.Length - 1))
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }

        static object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            //while (index-- >= 0)
            //    enm.MoveNext();
            //return enm.Current;

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }



        public static void SetValue(this SerializedProperty prop, object value)
        {
            if (prop == null) throw new System.ArgumentNullException("prop");

            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    prop.intValue = ConversionUtilities.ToInt(value);
                    break;
                case SerializedPropertyType.Boolean:
                    prop.boolValue = ConversionUtilities.ToBool(value);
                    break;
                case SerializedPropertyType.Float:
                    prop.floatValue = ConversionUtilities.ToSingle(value);
                    break;
                case SerializedPropertyType.String:
                    prop.stringValue = ConversionUtilities.ToString(value);
                    break;
                case SerializedPropertyType.Color:
                    prop.colorValue = ConversionUtilities.ToColor(value);
                    break;
                case SerializedPropertyType.ObjectReference:
                    prop.objectReferenceValue = value as UnityEngine.Object;
                    break;
                case SerializedPropertyType.LayerMask:
                    prop.intValue = (value is LayerMask) ? ((LayerMask) value).value : ConversionUtilities.ToInt(value);
                    break;
                case SerializedPropertyType.Enum:
                    prop.enumValueIndex = ConversionUtilities.ToInt(value);
                    prop.SetEnumValue(value);
                    break;
                case SerializedPropertyType.Vector2:
                    prop.vector2Value = ConversionUtilities.ToVector2(value);
                    break;
                case SerializedPropertyType.Vector3:
                    prop.vector3Value = ConversionUtilities.ToVector3(value);
                    break;
                case SerializedPropertyType.Vector4:
                    prop.vector4Value = ConversionUtilities.ToVector4(value);
                    break;
                case SerializedPropertyType.Rect:
                    prop.rectValue = (Rect) value;
                    break;
                case SerializedPropertyType.ArraySize:
                    prop.arraySize = ConversionUtilities.ToInt(value);
                    break;
                case SerializedPropertyType.Character:
                    prop.intValue = ConversionUtilities.ToInt(value);
                    break;
                case SerializedPropertyType.AnimationCurve:
                    prop.animationCurveValue = value as AnimationCurve;
                    break;
                case SerializedPropertyType.Bounds:
                    prop.boundsValue = (Bounds) value;
                    break;
                case SerializedPropertyType.Gradient:
                    throw new System.InvalidOperationException("Can not handle Gradient types.");
            }
        }

        public static void ResetToDefaultValue(this SerializedProperty property)
        {
            property.SetValue(GetDefaultValue(property.propertyType));
        }

        public static object GetDefaultValue(this SerializedPropertyType type)
        {
            switch (type)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Float:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.Enum:
                case SerializedPropertyType.ArraySize:
                    return 0;
                case SerializedPropertyType.Boolean:
                    return false;
                case SerializedPropertyType.String:
                case SerializedPropertyType.Character:
                    return "";
                case SerializedPropertyType.Color:
                    return Color.black;
                case SerializedPropertyType.ObjectReference:
                case SerializedPropertyType.ExposedReference:
                case SerializedPropertyType.ManagedReference:
                    return null;
                case SerializedPropertyType.Vector2:
                    return Vector2.zero;
                case SerializedPropertyType.Vector3:
                    return Vector3.zero;
                case SerializedPropertyType.Vector4:
                    return Vector4.zero;
                case SerializedPropertyType.Rect:
                    return Rect.zero;
                case SerializedPropertyType.AnimationCurve:
                    return new AnimationCurve();
                case SerializedPropertyType.Bounds:
                    return new Bounds();
                case SerializedPropertyType.Quaternion:
                    return Quaternion.identity;
                case SerializedPropertyType.Vector2Int:
                    return Vector2Int.zero;
                case SerializedPropertyType.Vector3Int:
                    return Vector3Int.zero;
                case SerializedPropertyType.RectInt:
                    return new RectInt();
                case SerializedPropertyType.BoundsInt:
                    return new BoundsInt();
                default:
                    throw new NotImplementedException($"No default value for {type} is defined.");
            }
        }

        public static void SetEnumValue<T>(this SerializedProperty prop, T value) where T : struct
        {
            if (prop == null) throw new System.ArgumentNullException("prop");
            if (prop.propertyType != SerializedPropertyType.Enum) throw new System.ArgumentException("SerializedProperty is not an enum type.", "prop");

            //var tp = typeof(T);
            //if(tp.IsEnum)
            //{
            //    prop.enumValueIndex = prop.enumNames.IndexOf(System.Enum.GetName(value));
            //}
            //else
            //{
            //    int i = ConvertUtil.ToInt(value);
            //    if (i < 0 || i >= prop.enumNames.Length) i = 0;
            //    prop.enumValueIndex = i;
            //}
            prop.intValue = ConversionUtilities.ToInt(value);
        }

        public static void SetEnumValue(this SerializedProperty prop, System.Enum value)
        {
            if (prop == null) throw new System.ArgumentNullException("prop");
            if (prop.propertyType != SerializedPropertyType.Enum) throw new System.ArgumentException("SerializedProperty is not an enum type.", "prop");

            if (value == null)
            {
                prop.enumValueIndex = 0;
                return;
            }

            //int i = prop.enumNames.IndexOf(System.Enum.GetName(value.GetType(), value));
            //if (i < 0) i = 0;
            //prop.enumValueIndex = i;
            prop.intValue = ConversionUtilities.ToInt(value);
        }

        public static void SetEnumValue(this SerializedProperty prop, object value)
        {
            if (prop == null) throw new System.ArgumentNullException("prop");
            if (prop.propertyType != SerializedPropertyType.Enum) throw new System.ArgumentException("SerializedProperty is not an enum type.", "prop");

            if (value == null)
            {
                prop.enumValueIndex = 0;
                return;
            }

            //var tp = value.GetType();
            //if (tp.IsEnum)
            //{
            //    int i = prop.enumNames.IndexOf(System.Enum.GetName(value));
            //    if (i < 0) i = 0;
            //    prop.enumValueIndex = i;
            //}
            //else
            //{
            //    int i = ConvertUtil.ToInt(value);
            //    if (i < 0 || i >= prop.enumNames.Length) i = 0;
            //    prop.enumValueIndex = i;
            //}
            prop.intValue = ConversionUtilities.ToInt(value);
        }

        public static object GetPropertyValue(this SerializedProperty prop)
        {
            if (prop == null) throw new System.ArgumentNullException("prop");

            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return prop.intValue;
                case SerializedPropertyType.Boolean:
                    return prop.boolValue;
                case SerializedPropertyType.Float:
                    return prop.floatValue;
                case SerializedPropertyType.String:
                    return prop.stringValue;
                case SerializedPropertyType.Color:
                    return prop.colorValue;
                case SerializedPropertyType.ObjectReference:
                    return prop.objectReferenceValue;
                case SerializedPropertyType.LayerMask:
                    return (LayerMask) prop.intValue;
                case SerializedPropertyType.Enum:
                    return prop.enumValueIndex;
                case SerializedPropertyType.Vector2:
                    return prop.vector2Value;
                case SerializedPropertyType.Vector3:
                    return prop.vector3Value;
                case SerializedPropertyType.Vector4:
                    return prop.vector4Value;
                case SerializedPropertyType.Rect:
                    return prop.rectValue;
                case SerializedPropertyType.ArraySize:
                    return prop.arraySize;
                case SerializedPropertyType.Character:
                    return (char) prop.intValue;
                case SerializedPropertyType.AnimationCurve:
                    return prop.animationCurveValue;
                case SerializedPropertyType.Bounds:
                    return prop.boundsValue;
                case SerializedPropertyType.Gradient:
                    throw new System.InvalidOperationException("Can not handle Gradient types.");
                case SerializedPropertyType.ManagedReference:
                    return prop.GetFieldInfo().GetValue(prop.GetTargetObjectOwner());
                default:
                    throw new NotImplementedException($"Case {prop.propertyType} not implemented in GetPropertyValue().");
            }
        }

        public static SerializedPropertyType GetPropertyType(System.Type tp)
        {
            if (tp == null) throw new System.ArgumentNullException("tp");

            if (tp.IsEnum) return SerializedPropertyType.Enum;

            var code = System.Type.GetTypeCode(tp);
            switch (code)
            {
                case System.TypeCode.SByte:
                case System.TypeCode.Byte:
                case System.TypeCode.Int16:
                case System.TypeCode.UInt16:
                case System.TypeCode.Int32:
                    return SerializedPropertyType.Integer;
                case System.TypeCode.Boolean:
                    return SerializedPropertyType.Boolean;
                case System.TypeCode.Single:
                    return SerializedPropertyType.Float;
                case System.TypeCode.String:
                    return SerializedPropertyType.String;
                case System.TypeCode.Char:
                    return SerializedPropertyType.Character;
                default:
                    {
                        if (typeof(Color).IsAssignableFrom(tp))
                            return SerializedPropertyType.Color;
                        else if (typeof(UnityEngine.Object).IsAssignableFrom(tp))
                            return SerializedPropertyType.ObjectReference;
                        else if (typeof(LayerMask).IsAssignableFrom(tp))
                            return SerializedPropertyType.LayerMask;
                        else if (typeof(Vector2).IsAssignableFrom(tp))
                            return SerializedPropertyType.Vector2;
                        else if (typeof(Vector3).IsAssignableFrom(tp))
                            return SerializedPropertyType.Vector3;
                        else if (typeof(Vector4).IsAssignableFrom(tp))
                            return SerializedPropertyType.Vector4;
                        else if (typeof(Quaternion).IsAssignableFrom(tp))
                            return SerializedPropertyType.Quaternion;
                        else if (typeof(Rect).IsAssignableFrom(tp))
                            return SerializedPropertyType.Rect;
                        else if (typeof(AnimationCurve).IsAssignableFrom(tp))
                            return SerializedPropertyType.AnimationCurve;
                        else if (typeof(Bounds).IsAssignableFrom(tp))
                            return SerializedPropertyType.Bounds;
                        else if (typeof(Gradient).IsAssignableFrom(tp))
                            return SerializedPropertyType.Gradient;
                    }
                    return SerializedPropertyType.Generic;

            }
        }

        public static double GetNumericValue(this SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return (double) prop.intValue;
                case SerializedPropertyType.Boolean:
                    return prop.boolValue ? 1d : 0d;
                case SerializedPropertyType.Float:
                    return prop.type == "double" ? prop.doubleValue : (double) prop.floatValue;
                case SerializedPropertyType.ArraySize:
                    return (double) prop.arraySize;
                case SerializedPropertyType.Character:
                    return (double) prop.intValue;
                default:
                    return 0d;
            }
        }

        public static void SetNumericValue(this SerializedProperty prop, double value)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    prop.intValue = (int) value;
                    break;
                case SerializedPropertyType.Boolean:
                    prop.boolValue = (System.Math.Abs(value) > Mathf.Epsilon);
                    break;
                case SerializedPropertyType.Float:
                    if (prop.type == "double")
                        prop.doubleValue = value;
                    else
                        prop.floatValue = (float) value;
                    break;
                case SerializedPropertyType.ArraySize:
                    prop.arraySize = (int) value;
                    break;
                case SerializedPropertyType.Character:
                    prop.intValue = (int) value;
                    break;
            }
        }

        public static bool IsNumericValue(this SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Boolean:
                case SerializedPropertyType.Float:
                case SerializedPropertyType.ArraySize:
                case SerializedPropertyType.Character:
                    return true;
                default:
                    return false;
            }
        }



        public static int GetChildPropertyCount(SerializedProperty property, bool includeGrandChildren = false)
        {
            var pstart = property.Copy();
            var pend = property.GetEndProperty();
            int cnt = 0;

            pstart.Next(true);
            while (!SerializedProperty.EqualContents(pstart, pend))
            {
                cnt++;
                pstart.Next(includeGrandChildren);
            }

            return cnt;
        }

        #endregion

        #region Serialized Field Helpers
        public static System.Reflection.FieldInfo GetFieldInfo(this SerializedProperty prop)
        {
            if (prop == null) return null;

            var tp = GetTargetType(prop.serializedObject);
            if (tp == null) return null;

            var path = prop.propertyPath.Replace(".Array.data[", "[");
            var elements = path.Split('.');
            System.Reflection.FieldInfo field;
            foreach (var element in elements.Take(elements.Length - 1))
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));

                    field = tp.GetMember(elementName, MemberTypes.Field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault() as System.Reflection.FieldInfo;
                    if (field == null) return null;
                    tp = field.FieldType;
                }
                else
                {
                    field = tp.GetMember(element, MemberTypes.Field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault() as System.Reflection.FieldInfo;
                    if (field == null) return null;
                    tp = field.FieldType;
                }
            }

            return tp.GetField(elements.Last(), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            //return null;
        }
        #endregion

        #region Path
        public static string GetFullPathForAssetPath(string assetPath)
        {
            return Application.dataPath.EnsureNotEndsWith("Assets") + "/" + assetPath.EnsureNotStartWith("/");
        }
        #endregion
    }

}
