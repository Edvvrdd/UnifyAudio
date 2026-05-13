using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FMODUnity;
using UnifyAudio.Parameters;
using ParameterType = UnifyAudio.Parameters.ParameterType;

namespace UnifyAudio.Editor
{
    [CustomEditor(typeof(UnifyInstanceManager))]
    public class UnifyInstanceControllerEditor : UnityEditor.Editor
    {
        private SerializedProperty _eventProperty;
        private SerializedProperty _parametersProperty;
        private SerializedProperty _playOnStart;
        private SerializedProperty _playOnEnable;
        private SerializedProperty _resumeIfPaused;
        private SerializedProperty _pauseOnDisable;
        private SerializedProperty _releaseOnStop;
        private SerializedProperty _playSignals;
        private SerializedProperty _pauseSignals;
        private SerializedProperty _stopSignals;

        private FMOD.GUID _lastKnownGuid;
        private List<bool> _parameterFoldouts = new();

        private void OnEnable()
        {
            _eventProperty = serializedObject.FindProperty("_event");
            _parametersProperty = serializedObject.FindProperty("_parameters");
            _playOnStart = serializedObject.FindProperty("_playOnStart");
            _playOnEnable = serializedObject.FindProperty("_playOnEnable");
            _resumeIfPaused = serializedObject.FindProperty("_resumeIfPaused");
            _pauseOnDisable = serializedObject.FindProperty("_pauseOnDisable");
            _releaseOnStop = serializedObject.FindProperty("_releaseOnStop");
            _playSignals = serializedObject.FindProperty("_playSignals");
            _pauseSignals = serializedObject.FindProperty("_pauseSignals");
            _stopSignals = serializedObject.FindProperty("_stopSignals");

            UnifyInstanceManager controller = (UnifyInstanceManager)target;
            _lastKnownGuid = controller.GetEventGuid();

            if (_parametersProperty.arraySize == 0)
                SyncParametersFromEvent();
        }

        public override void OnInspectorGUI()
        {
            UnifyInstanceManager controller = (UnifyInstanceManager)target;
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(controller), typeof(UnifyInstanceManager), false);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Event", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_eventProperty);
            serializedObject.ApplyModifiedProperties();

            FMOD.GUID currentGuid = controller.GetEventGuid();
            if (!currentGuid.Equals(_lastKnownGuid))
            {
                _lastKnownGuid = currentGuid;
                SyncParametersFromEvent();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);

            if (_parametersProperty.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No local parameters found on this event.", MessageType.Info);
            }
            else
            {
                while (_parameterFoldouts.Count < _parametersProperty.arraySize)
                    _parameterFoldouts.Add(false);
                while (_parameterFoldouts.Count > _parametersProperty.arraySize)
                    _parameterFoldouts.RemoveAt(_parameterFoldouts.Count - 1);

                for (int i = 0; i < _parametersProperty.arraySize; i++)
                {
                    SerializedProperty binding = _parametersProperty.GetArrayElementAtIndex(i);
                    SerializedProperty paramName = binding.FindPropertyRelative("ParameterName");
                    SerializedProperty paramValue = binding.FindPropertyRelative("Value");
                    SerializedProperty minProp = binding.FindPropertyRelative("Min");
                    SerializedProperty maxProp = binding.FindPropertyRelative("Max");
                    SerializedProperty defaultProp = binding.FindPropertyRelative("DefaultValue");
                    SerializedProperty typeProp = binding.FindPropertyRelative("ParameterType");
                    SerializedProperty labelsProp = binding.FindPropertyRelative("Labels");

                    ParameterType pType = (ParameterType)typeProp.enumValueIndex;
                    bool isLabeled = pType == ParameterType.Labeled;

                    EditorGUILayout.BeginHorizontal();

                    string foldoutLabel = paramName.stringValue;
                    if (string.IsNullOrEmpty(foldoutLabel))
                        foldoutLabel = "(unnamed)";

                    _parameterFoldouts[i] = EditorGUILayout.Foldout(
                        _parameterFoldouts[i],
                        foldoutLabel,
                        true
                    );

                    EditorGUI.BeginChangeCheck();
                    var assigned = EditorGUILayout.ObjectField(
                        paramValue.objectReferenceValue,
                        typeof(UnifyParameter),
                        false
                    ) as UnifyParameter;
                    if (EditorGUI.EndChangeCheck())
                        paramValue.objectReferenceValue = assigned;

                    EditorGUILayout.EndHorizontal();

                    if (_parameterFoldouts[i])
                    {
                        EditorGUI.indentLevel++;

                        string typeLabel = GetTypeShortName(pType);
                        string rangeLabel = $"{minProp.floatValue:F2} — {maxProp.floatValue:F2}";

                        using (new EditorGUI.DisabledScope(true))
                        {
                            EditorGUILayout.TextField("Type", typeLabel);
                            EditorGUILayout.TextField("Range", rangeLabel);
                            EditorGUILayout.FloatField("Default", defaultProp.floatValue);

                            if (isLabeled && labelsProp.arraySize > 0)
                            {
                                EditorGUILayout.Space(2);
                                EditorGUILayout.LabelField("Labels", EditorStyles.miniBoldLabel);
                                EditorGUI.indentLevel++;
                                for (int j = 0; j < labelsProp.arraySize; j++)
                                {
                                    string label = labelsProp.GetArrayElementAtIndex(j).stringValue;
                                    EditorGUILayout.LabelField($"{j} → {label}");
                                }
                                EditorGUI.indentLevel--;
                            }
                        }

                        EditorGUI.indentLevel--;
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Play Signals", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_playOnStart);
            EditorGUILayout.PropertyField(_playOnEnable);
            EditorGUILayout.PropertyField(_resumeIfPaused);
            EditorGUILayout.PropertyField(_playSignals);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Pause Signals", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_pauseOnDisable);
            EditorGUILayout.PropertyField(_pauseSignals);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Stop Signals", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_releaseOnStop);
            EditorGUILayout.PropertyField(_stopSignals);

            serializedObject.ApplyModifiedProperties();
        }

        private void SyncParametersFromEvent()
        {
            UnifyInstanceManager controller = (UnifyInstanceManager)target;
            FMOD.GUID guid = controller.GetEventGuid();

            EditorEventRef eventRef = FMODUnity.EventManager.EventFromGUID(guid);

            if (eventRef == null)
            {
                _parametersProperty.ClearArray();
                serializedObject.ApplyModifiedProperties();
                return;
            }

            var existingValues = new Dictionary<string, Object>();
            for (int i = 0; i < _parametersProperty.arraySize; i++)
            {
                SerializedProperty binding = _parametersProperty.GetArrayElementAtIndex(i);
                string existingName = binding.FindPropertyRelative("ParameterName").stringValue;
                Object existingValue = binding.FindPropertyRelative("Value").objectReferenceValue;
                if (!string.IsNullOrEmpty(existingName))
                    existingValues[existingName] = existingValue;
            }

            _parametersProperty.ClearArray();

            for (int i = 0; i < eventRef.LocalParameters.Count; i++)
            {
                var paramRef = eventRef.LocalParameters[i];
                if (paramRef == null) continue;

                _parametersProperty.InsertArrayElementAtIndex(i);
                SerializedProperty binding = _parametersProperty.GetArrayElementAtIndex(i);
                binding.FindPropertyRelative("ParameterName").stringValue = paramRef.Name;

                existingValues.TryGetValue(paramRef.Name, out Object preservedValue);
                binding.FindPropertyRelative("Value").objectReferenceValue = preservedValue;

                binding.FindPropertyRelative("Min").floatValue = paramRef.Min;
                binding.FindPropertyRelative("Max").floatValue = paramRef.Max;
                binding.FindPropertyRelative("DefaultValue").floatValue = paramRef.Default;
                binding.FindPropertyRelative("ParameterType").enumValueIndex = (int)(ParameterType)paramRef.Type;

                SerializedProperty labelsProp = binding.FindPropertyRelative("Labels");
                labelsProp.ClearArray();
                if (paramRef.Labels != null)
                {
                    for (int j = 0; j < paramRef.Labels.Length; j++)
                    {
                        labelsProp.InsertArrayElementAtIndex(j);
                        labelsProp.GetArrayElementAtIndex(j).stringValue = paramRef.Labels[j];
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
        }

        private static string GetTypeShortName(ParameterType type) => type switch
        {
            ParameterType.Continuous => "Continuous",
            ParameterType.Discrete => "Discrete",
            ParameterType.Labeled => "Labeled",
            _ => "Unknown"
        };
    }
}
