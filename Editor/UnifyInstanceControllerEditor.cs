using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FMODUnity;
using UnifyAudio.Parameters;

namespace UnifyAudio.Editor
{
    [CustomEditor(typeof(UnifyInstanceManager))]
    public class UnifyInstanceControllerEditor : UnityEditor.Editor
    {
        private SerializedProperty _eventProperty;
        private SerializedProperty _parametersProperty;
        private SerializedProperty _playOnStart;
        private SerializedProperty _playOnEnable;
        private SerializedProperty _pauseOnDisable;
        private SerializedProperty _stopAndReleaseOnDestroy;
        private SerializedProperty _releaseOnStop;
        private SerializedProperty _playSignals;
        private SerializedProperty _pauseSignals;
        private SerializedProperty _stopSignals;

        private FMOD.GUID _lastKnownGuid;

        private void OnEnable()
        {
            _eventProperty = serializedObject.FindProperty("_event");
            _parametersProperty = serializedObject.FindProperty("_parameters");
            _playOnStart = serializedObject.FindProperty("_playOnStart");
            _playOnEnable = serializedObject.FindProperty("_playOnEnable");
            _pauseOnDisable = serializedObject.FindProperty("_pauseOnDisable");
            _stopAndReleaseOnDestroy = serializedObject.FindProperty("_stopAndReleaseOnDestroy");
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
                for (int i = 0; i < _parametersProperty.arraySize; i++)
                {
                    SerializedProperty binding = _parametersProperty.GetArrayElementAtIndex(i);
                    SerializedProperty paramName = binding.FindPropertyRelative("ParameterName");
                    SerializedProperty paramValue = binding.FindPropertyRelative("Value");

                    EditorGUILayout.BeginHorizontal();

                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.TextField(paramName.stringValue, GUILayout.Width(160));
                    EditorGUI.EndDisabledGroup();

                    EditorGUI.BeginChangeCheck();
                    var assigned = EditorGUILayout.ObjectField(
                        paramValue.objectReferenceValue,
                        typeof(UnifyParameter),
                        false) as UnifyParameter;

                    if (EditorGUI.EndChangeCheck())
                        paramValue.objectReferenceValue = assigned;

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Auto Triggers", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_playOnStart);
            EditorGUILayout.PropertyField(_playOnEnable);
            EditorGUILayout.PropertyField(_pauseOnDisable);
            EditorGUILayout.PropertyField(_stopAndReleaseOnDestroy);
            EditorGUILayout.PropertyField(_releaseOnStop);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Signal Slots", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_playSignals);
            EditorGUILayout.PropertyField(_pauseSignals);
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
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
        }
    }
}
