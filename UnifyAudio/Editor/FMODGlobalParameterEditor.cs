using UnityEngine;
using UnityEditor;
using FMODUnity;
using UnifyAudio.Parameters;

namespace UnifyAudio.Editor
{
    [CustomEditor(typeof(UnifyGlobalParameter))]
    public class FMODGlobalParameterEditor : UnityEditor.Editor
    {
        private SerializedProperty _parameterProperty;
        private string _lastKnownParameter;

        private void OnEnable()
        {
            _parameterProperty = serializedObject.FindProperty("Parameter");
            _lastKnownParameter = _parameterProperty.stringValue;

            UnifyGlobalParameter asset = (UnifyGlobalParameter)target;
            asset.CacheParameterDescription();
        }

        public override void OnInspectorGUI()
        {
            UnifyGlobalParameter asset = (UnifyGlobalParameter)target;
            serializedObject.Update();

            EditorGUILayout.PropertyField(_parameterProperty);
            serializedObject.ApplyModifiedProperties();

            if (_parameterProperty.stringValue != _lastKnownParameter)
            {
                _lastKnownParameter = _parameterProperty.stringValue;
                asset.CacheParameterDescription();
                Repaint();
            }

            if (!string.IsNullOrEmpty(asset.Parameter))
            {
                EditorGUILayout.Space();
                DrawParameterInfo(asset);
            }
        }

        private void DrawParameterInfo(UnifyGlobalParameter asset)
        {
            EditorGUILayout.LabelField("Parameter info", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField("Type", GetParameterTypeLabel(asset.ParameterType));
                EditorGUILayout.FloatField("Min", asset.Min);
                EditorGUILayout.FloatField("Max", asset.Max);
                EditorGUILayout.FloatField("Default", asset.DefaultValue);

                if (asset.ParameterType == ParameterType.Labeled
                    && asset.Labels != null
                    && asset.Labels.Length > 0)
                {
                    EditorGUILayout.Space(4);
                    EditorGUILayout.LabelField("Labels", EditorStyles.miniBoldLabel);
                    for (int i = 0; i < asset.Labels.Length; i++)
                        EditorGUILayout.LabelField($"  {i} -> {asset.Labels[i]}");
                }
            }
        }

        public static string GetParameterTypeLabel(ParameterType type) => type switch
        {
            ParameterType.Continuous => "Continuous (float)",
            ParameterType.Discrete => "Discrete (int)",
            ParameterType.Labeled => "Labeled (enum)",
            _ => "Unknown"
        };
    }
}
