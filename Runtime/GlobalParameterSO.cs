using System;
using UnityEngine;
using FMODUnity;

namespace UnifyAudio.Parameters
{
    [CreateAssetMenu(menuName = "UnifyAudio/Global Parameter")]
    public class UnifyGlobalParameter : UnifyParameterAsset
    {
        [ParamRef]
        public string Parameter;

        [SerializeField, HideInInspector] private float _min;
        [SerializeField, HideInInspector] private float _max;
        [SerializeField, HideInInspector] private float _defaultValue;
        [SerializeField, HideInInspector] private ParameterType _parameterType;
        [SerializeField, HideInInspector] private string[] _labels = Array.Empty<string>();

        public float Min => _min;
        public float Max => _max;
        public float DefaultValue => _defaultValue;
        public ParameterType ParameterType => _parameterType;
        public string[] Labels => _labels;

#if UNITY_EDITOR
        public static Func<UnifyGlobalParameter, bool> EditorCacheDelegate;

        public void ApplyCachedDescription(float min, float max, float defaultValue, ParameterType type, string[] labels)
        {
            _min = min;
            _max = max;
            _defaultValue = defaultValue;
            _parameterType = type;
            _labels = labels;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

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
            if (EditorCacheDelegate != null)
                EditorCacheDelegate(this);
#else
            FMOD.Studio.PARAMETER_DESCRIPTION description;
            FMOD.RESULT result = RuntimeManager.StudioSystem
                .getParameterDescriptionByName(Parameter, out description);

            if (result != FMOD.RESULT.OK)
            {
                Debug.LogWarning($"[UnifyAudio] Could not find global parameter '{Parameter}' " +
                                 $"on asset '{name}'. FMOD result: {result}.");
                return;
            }

            _min = description.minimum;
            _max = description.maximum;
            _defaultValue = description.defaultvalue;

            if ((description.flags & FMOD.Studio.PARAMETER_FLAGS.LABELED) != 0)
            {
                _parameterType = ParameterType.Labeled;
                var labels = new System.Collections.Generic.List<string>();
                int index = 0;
                while (true)
                {
                    string label;
                    if (RuntimeManager.StudioSystem.getParameterLabelByID(description.id, index, out label) != FMOD.RESULT.OK)
                        break;
                    labels.Add(label);
                    index++;
                }
                _labels = labels.ToArray();
            }
            else if ((description.flags & FMOD.Studio.PARAMETER_FLAGS.DISCRETE) != 0)
            {
                _parameterType = ParameterType.Discrete;
                _labels = Array.Empty<string>();
            }
            else
            {
                _parameterType = ParameterType.Continuous;
                _labels = Array.Empty<string>();
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