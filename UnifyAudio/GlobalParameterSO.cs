using System;
using UnityEngine;
using FMODUnity;

namespace UnifyAudio.Parameters
{
    [CreateAssetMenu(menuName = "UnifyAudio/Global Parameter")]
    public class FMODGlobalParameter : UnifyParameterAsset
    {
        [ParamRef]
        public string Parameter;

        [SerializeField, HideInInspector] private float _min;
        [SerializeField, HideInInspector] private float _max;
        [SerializeField, HideInInspector] private float _defaultValue;
        [SerializeField, HideInInspector] private FMODUnity.ParameterType _parameterType;
        [SerializeField, HideInInspector] private string[] _labels = Array.Empty<string>();

        public float Min => _min;
        public float Max => _max;
        public float DefaultValue => _defaultValue;
        public FMODUnity.ParameterType ParameterType => _parameterType;
        public string[] Labels => _labels;

        protected override void OnEnable()
        {
            base.OnEnable();
            CacheParameterDescription();
        }

        public void CacheParameterDescription()
        {
            if (string.IsNullOrEmpty(Parameter))
                return;

#if UNITY_EDITOR
            EditorParamRef paramRef = FMODUnity.EventManager.Parameters
                .Find(p => p.Name == Parameter);

            if (paramRef != null)
            {
                _min = paramRef.Min;
                _max = paramRef.Max;
                _defaultValue = paramRef.Default;
                _parameterType = paramRef.Type;
                _labels = paramRef.Labels;
                UnityEditor.EditorUtility.SetDirty(this);
            }
            else
            {
                Debug.LogWarning($"[UnifyAudio] Could not find global parameter '{Parameter}' " +
                                 $"on asset '{name}'. Ensure your FMOD project is built and banks are imported.");
            }
#endif
        }

        public void SetValue(float value)
        {
            WarnIfOffMainThread(name);

            if (string.IsNullOrEmpty(Parameter))
            {
                Debug.LogWarning($"[UnifyAudio] SetValue called on '{name}' but no parameter is assigned.");
                return;
            }

            FMOD.Studio.PARAMETER_DESCRIPTION description;
            FMOD.RESULT lookupResult = RuntimeManager.StudioSystem
                .getParameterDescriptionByName(Parameter, out description);

            if (lookupResult != FMOD.RESULT.OK)
            {
                Debug.LogWarning($"[UnifyAudio] Could not find parameter '{Parameter}' " +
                                 $"on asset '{name}'. FMOD result: {lookupResult}.");
                return;
            }

            FMOD.RESULT result = RuntimeManager.StudioSystem
                .setParameterByID(description.id, value);

            if (result != FMOD.RESULT.OK)
            {
                Debug.LogWarning($"[UnifyAudio] Failed to set global parameter '{Parameter}' " +
                                 $"on asset '{name}'. FMOD result: {result}.");
            }
        }
    }
}