using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StreetLarpers.Instituto.Editor
{
    internal static class InstitutoEditorUtils
    {
        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }

        // --- Asignación de campos privados [SerializeField] sin hacerlos públicos ---

        public static void SetRef(Object target, string field, Object value) =>
            Edit(target, field, p => p.objectReferenceValue = value);

        public static void SetFloat(Object target, string field, float value) =>
            Edit(target, field, p => p.floatValue = value);

        public static void SetInt(Object target, string field, int value) =>
            Edit(target, field, p => p.intValue = value);

        public static void SetEnum(Object target, string field, int value) =>
            Edit(target, field, p => p.enumValueIndex = value);

        public static void SetVector2(Object target, string field, Vector2 value) =>
            Edit(target, field, p => p.vector2Value = value);

        public static void SetRefArray<T>(Object target, string field, IList<T> values) where T : Object =>
            Edit(target, field, p =>
            {
                p.arraySize = values.Count;
                for (int i = 0; i < values.Count; i++)
                    p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            });

        private static void Edit(Object target, string field, System.Action<SerializedProperty> apply)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(field);
            if (property == null)
            {
                Debug.LogError($"[Instituto] El campo '{field}' no existe en {target.GetType().Name}.", target);
                return;
            }
            apply(property);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
