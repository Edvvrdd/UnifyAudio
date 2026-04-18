using UnityEngine;
using UnityEditor;
using UnifyAudio.Parameters;

namespace UnifyAudio.Editor
{
    [CustomEditor(typeof(UnifyParameter))]
    public class FMODLocalParameterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                UnifyParameter asset = (UnifyParameter)target;
                using (new EditorGUI.DisabledScope(true))
                    EditorGUILayout.FloatField("Current value", asset.Value);

                Repaint();
            }
        }
    }
}