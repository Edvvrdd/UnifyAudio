using UnityEngine;
using UnityEditor;
using UnifyAudio.Parameters;

namespace UnifyAudio.Editor
{
    [CustomEditor(typeof(FMODLocalParameter))]
    public class FMODLocalParameterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                FMODLocalParameter asset = (FMODLocalParameter)target;
                using (new EditorGUI.DisabledScope(true))
                    EditorGUILayout.FloatField("Current value", asset.Value);

                Repaint();
            }
        }
    }
}