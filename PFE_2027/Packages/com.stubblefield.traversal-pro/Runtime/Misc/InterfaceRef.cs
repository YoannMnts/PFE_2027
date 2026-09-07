using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace TraversalPro
{
    [System.Serializable]
    public struct InterfaceRef<T> : ISerializationCallbackReceiver
        where T : class
    {
        [SerializeField] Object source;
        T _value;
        
        public T Value
        {
            get
            {
                if (_value != null) return _value;
                Object s = source;
                if (s is GameObject go) s = go.GetComponent(typeof(T)) as Object;
                if (s != null && typeof(T).IsInstanceOfType(s)) _value = Unsafe.As<T>(s);
                return _value;
            }
            set => SetValue(value, out _value, out source);
        }

        public InterfaceRef(T value)
        {
            SetValue(value, out _value, out source);
        }

        public void OnBeforeSerialize()
        {
            // NE RIEN FAIRE qui touche au parametre generique T ici.
            // Unity invoque cette callback via un runtime-invoke sur l'instanciation
            // generique PARTAGEE du struct; dans ce contexte le contexte generique de T
            // n'est pas resoluble et tout acces a T (typeof(T), isinst T, GetComponent<T>)
            // plante Mono (mono_class_fill_runtime_generic_context) a l'import du prefab.
            // 'source' se serialise seul via [SerializeField]; la resolution T (y compris
            // GameObject -> composant) est faite paresseusement dans le getter Value.
        }

        public void OnAfterDeserialize()
        {
            // Fix crash: le cast 'source as T' etait execute ici, sur le thread de
            // deserialisation de Unity, pendant l'import du prefab -> crash natif Mono.
            // On le differe au getter (Value), donc sur le main thread, en lazy.
            _value = null;
        }

        static void SetValue(T value, out T _value, out Object source)
        {
            if (value is Object obj)
            {
                source = obj;
                _value = value;
            }
            else if (value == null)
            {
                source = null;
                _value = null;
            }
            else
            {
                throw new ArgumentException($"{value.GetType().Name} is not a UnityEngine.Object.");
            }
        }

        public static implicit operator T(InterfaceRef<T> value) => value.Value;
        public static implicit operator InterfaceRef<T>(T value) => new(value);
    }
     
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(InterfaceRef<>))]
    public class RefEditor : PropertyDrawer  
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            PropertyField field = new(property.FindPropertyRelative("source"));
            field.label = $"{property.displayName}";
            field.tooltip = property.tooltip;
            field.RegisterValueChangeCallback(e =>
            {
                ObjectField objectField = field.Query<ObjectField>();
                if (objectField != null)
                {
                    if (!objectField.value)
                    {
                        Label label = objectField.Query<Label>(null, "unity-object-field-display__label");
                        if (label != null)
                        {
                            string typeName = fieldInfo.FieldType.GetGenericArguments()[0].Name;
                            label.text = $"None ({typeName})";
                        }
                    }
                }
            });
            // setting objectField.objectType to the interface type prevents dropping GameObjects
            return field;
        }
    }
    #endif
}