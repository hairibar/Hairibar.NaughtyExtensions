using NaughtyAttributes.Editor;
using UnityEditor;
using UnityEngine;

namespace Hairibar.NaughtyExtensions.Editor
{
    [CustomPropertyDrawer(typeof(QuadraticSliderAttribute))]
    public class QuadraticSliderDrawer : PropertyDrawerBase
    {
        protected override void OnGUI_Internal(Rect rect, SerializedProperty property, GUIContent label)
        {
            QuadraticSliderAttribute quadraticAttribute = PropertyUtility.GetAttribute<QuadraticSliderAttribute>(property);
            if (quadraticAttribute != null)
            {
                NonLinearSliderDrawer.Draw(rect, property, quadraticAttribute.Min, quadraticAttribute.Max, GetQuadraticFunction(quadraticAttribute.Power), label);
            }
        }

        static NonLinearSliderDrawer.Function GetQuadraticFunction(float power)
        {
            return new NonLinearSliderDrawer.Function
            {
                function = (x) => Mathf.Pow(x, power),
                backwardsFunction = (x) => Mathf.Pow(x, 1f / power)
            };
        }
    }
}
