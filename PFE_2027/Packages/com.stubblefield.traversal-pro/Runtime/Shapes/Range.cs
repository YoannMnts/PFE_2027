using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace TraversalPro
{
    [System.Serializable]
    public struct Range
    {
        [Delayed] public float min;
        [Delayed] public float max;

        public Range(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }
    
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Range))]
    public class RangePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            Vector2Field field = new(property.displayName);
            field.tooltip = property.tooltip;
            field.AddToClassList("unity-base-field__aligned");
            FloatField x = field.Q<FloatField>("unity-x-input");
            if (x != null)
            {
                x.BindProperty(property.FindPropertyRelative("min"));
                x.label = "min";
                x.labelElement.style.minWidth = 25;
                x.style.flexBasis = new StyleLength(StyleKeyword.Auto);
            }
            FloatField y = field.Q<FloatField>("unity-y-input");
            if (y != null)
            {
                y.BindProperty(property.FindPropertyRelative("max"));
                y.label = "max";
                y.labelElement.style.minWidth = 30;
                y.style.flexBasis = new StyleLength(StyleKeyword.Auto);
            }
            return field;
        }
    }
    #endif
}