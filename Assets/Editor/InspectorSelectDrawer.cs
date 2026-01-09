using System;
using System.Collections.Generic;
using System.Linq;
using Runtime.UI;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomPropertyDrawer(typeof(SelectTypeAttribute))]
    public class InspectorSelectDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var selectAttribute = (SelectTypeAttribute) attribute;
            Type baseType = selectAttribute.BaseType;
            if (baseType == null)
                return;

            List<Type> availableTypes = GetDerivedTypes(baseType);
            availableTypes.Insert(0, null);

            Type currentValue = GetTargetObjectOfProperty(property);
            string[] typeNames = availableTypes.Select(t => t == null ? "None (null)" : $"typeOf({t.Name})").ToArray();

            // Находим текущий индекс
            int currentIndex = availableTypes.IndexOf(currentValue);
            if (currentIndex == -1) currentIndex = 0;

            // Рисуем выпадающий список
            EditorGUI.BeginProperty(position, label, property);
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label);
                var helpBoxPosition = new Rect(position)
                {
                    x = position.x + EditorGUIUtility.labelWidth,
                    height = EditorGUIUtility.singleLineHeight * 2
                };
                EditorGUI.HelpBox(helpBoxPosition, "Only string values is supported.", MessageType.Error);
            }
            else
            {
                Rect popupPosition = EditorGUI.PrefixLabel(position, label);
                int newIndex = EditorGUI.Popup(popupPosition, currentIndex, typeNames);

                if (newIndex != currentIndex)
                {
                    if (availableTypes[newIndex] == null)
                    {
                        property.managedReferenceValue = null;
                    }
                    else
                    {
                        Type selectedType = availableTypes[newIndex];
                        property.stringValue = selectedType.AssemblyQualifiedName;
                    }

                    property.serializedObject.ApplyModifiedProperties();
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
                return EditorGUIUtility.singleLineHeight * 2;
            return EditorGUIUtility.singleLineHeight;
        }


        private static List<Type> GetDerivedTypes(Type baseType)
        {
            var types = new List<Type>();

            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in allAssemblies)
            {
                try
                {
                    var assemblyTypes = assembly.GetTypes()
                        .Where(t => baseType.IsAssignableFrom(t) && !t.IsInterface)
                        .Where(t => !t.IsAbstract)
                        .Where(t => !t.IsGenericTypeDefinition);

                    types.AddRange(assemblyTypes);
                }
                catch (System.Reflection.ReflectionTypeLoadException)
                {
                }
            }

            return types.OrderBy(t => t.Name).ToList();
        }

        private static Type GetTargetObjectOfProperty(SerializedProperty prop)
        {
            if (prop.propertyType != SerializedPropertyType.String)
                return null;

            var value = prop.stringValue;
            if (string.IsNullOrEmpty(value))
                return null;

            var type = Type.GetType(value);
            if (type == null)
                prop.stringValue = null;

            return type;
        }


        private static Type GetValue(object source, string name)
        {
            if (source == null) return null;

            var type = source.GetType();
            var field = type.GetField(name,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance);

            if (field != null) return field.GetValue(source) as Type;

            var property = type.GetProperty(name,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.IgnoreCase);

            if (property != null) return property.GetValue(source, null) as Type;

            return null;
        }
    }
}