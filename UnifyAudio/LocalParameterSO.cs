using System;
using UnityEngine;

namespace UnifyAudio.Parameters
{
    [CreateAssetMenu(menuName = "UnifyAudio/Parameter Value")]
    public class UnifyParameter : UnifyParameterAsset
    {
        private float _value;
        public float Value => _value;

        public event Action<float> OnValueChanged;

        protected override void OnEnable()
        {
            base.OnEnable();
            OnValueChanged = null;
        }

        public void SetValue(float value)
        {
            WarnIfOffMainThread(name);
            _value = value;
            OnValueChanged?.Invoke(value);
        }
    }
}