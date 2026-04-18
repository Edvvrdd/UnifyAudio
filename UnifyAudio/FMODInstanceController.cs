using System;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using UnifyAudio.Parameters;

namespace UnifyAudio
{
    public class UnifyInstanceManager : MonoBehaviour
    {
        [SerializeField] private EventReference _event;
        [SerializeField] private List<FMODParameterBinding> _parameters = new();
        [SerializeField] private bool _playOnStart;
        [SerializeField] private bool _playOnEnable;
        [SerializeField] private bool _pauseOnDisable;
        [SerializeField] private bool _stopAndReleaseOnDestroy = true;
        [SerializeField] private string _triggerTag;
        [SerializeField] private bool _playOnTriggerEnter;
        [SerializeField] private bool _pauseOnTriggerEnter;
        [SerializeField] private bool _stopOnTriggerEnter;
        [SerializeField] private List<UnifySignal> _playSignals = new();
        [SerializeField] private List<UnifySignal> _pauseSignals = new();
        [SerializeField] private List<UnifySignal> _stopSignals = new();

        private EventInstance _instance;
        private readonly List<Action<float>> _parameterCallbacks = new();

        private void Awake()
        {
            _instance = RuntimeManager.CreateInstance(_event);
            RuntimeManager.AttachInstanceToGameObject(_instance, gameObject);

            foreach (var binding in _parameters)
            {
                if (binding.Value != null)
                {
                    string paramName = binding.ParameterName;
                    Action<float> callback = value => ApplyParameter(paramName, value);
                    _parameterCallbacks.Add(callback);
                    binding.Value.OnValueChanged += callback;
                }
                else
                {
                    _parameterCallbacks.Add(null);
                }
            }

            foreach (var signal in _playSignals)
                if (signal != null) signal.OnFired += Play;

            foreach (var signal in _pauseSignals)
                if (signal != null) signal.OnFired += Pause;

            foreach (var signal in _stopSignals)
                if (signal != null) signal.OnFired += Stop;
        }

        private void Start()
        {
            if (_playOnStart)
                Play();
        }

        private void OnEnable()
        {
            if (_playOnEnable && _instance.isValid())
                Play();
        }

        private void OnDisable()
        {
            if (_pauseOnDisable && _instance.isValid())
                Pause();
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _parameters.Count; i++)
            {
                if (_parameters[i].Value != null && _parameterCallbacks[i] != null)
                    _parameters[i].Value.OnValueChanged -= _parameterCallbacks[i];
            }

            foreach (var signal in _playSignals)
                if (signal != null) signal.OnFired -= Play;

            foreach (var signal in _pauseSignals)
                if (signal != null) signal.OnFired -= Pause;

            foreach (var signal in _stopSignals)
                if (signal != null) signal.OnFired -= Stop;

            if (_stopAndReleaseOnDestroy && _instance.isValid())
            {
                _instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                _instance.release();
            }
            else if (_instance.isValid())
            {
                _instance.release();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!string.IsNullOrEmpty(_triggerTag) && !other.CompareTag(_triggerTag))
                return;

            if (_playOnTriggerEnter) Play();
            if (_pauseOnTriggerEnter) Pause();
            if (_stopOnTriggerEnter) Stop();
        }

        public void Play()
        {
            if (!_instance.isValid())
            {
                Debug.LogWarning($"[FMODToolkit] Play called on '{name}' but instance is not valid.");
                return;
            }
            _instance.start();
        }

        public void Pause()
        {
            if (!_instance.isValid())
            {
                Debug.LogWarning($"[FMODToolkit] Pause called on '{name}' but instance is not valid.");
                return;
            }
            _instance.setPaused(true);
        }

        public void Stop()
        {
            if (!_instance.isValid())
            {
                Debug.LogWarning($"[FMODToolkit] Stop called on '{name}' but instance is not valid.");
                return;
            }
            _instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        public FMOD.RESULT ApplyParameter(string parameterName, float value)
        {
            if (!_instance.isValid())
            {
                Debug.LogWarning($"[FMODToolkit] ApplyParameter called on '{name}' but instance is not valid.");
                return FMOD.RESULT.ERR_INVALID_HANDLE;
            }
            return _instance.setParameterByName(parameterName, value);
        }

#if UNITY_EDITOR
        public FMOD.GUID GetEventGuid() => _event.Guid;

        public void SyncParametersFromEvent()
        {
            if (_event.IsNull)
            {
                _parameters.Clear();
                return;
            }

            EditorEventRef eventRef = FMODUnity.EventManager.EventFromGUID(_event.Guid);
            if (eventRef == null) return;

            List<FMODParameterBinding> synced = new();

            foreach (var paramRef in eventRef.LocalParameters)
            {
                if (paramRef == null) continue;

                FMODParameterBinding existing = _parameters.Find(p => p.ParameterName == paramRef.Name);
                synced.Add(new FMODParameterBinding
                {
                    ParameterName = paramRef.Name,
                    Value = existing != null ? existing.Value : null
                });
            }

            _parameters = synced;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}