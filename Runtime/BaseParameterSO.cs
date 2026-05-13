using UnityEngine;

namespace UnifyAudio.Parameters
{
    public enum ParameterType
    {
        Continuous,
        Discrete,
        Labeled,
    }

    public abstract class UnifyParameterAsset : ScriptableObject
    {
        // FMOD Studio API is thread-safe at runtime — Unity's FMOD integration initializes with
        // FMOD.Studio.INITFLAGS.DEFERRED_CALLBACKS (not SYNCHRONOUS_UPDATE), so calls from any
        // thread are queued and processed on the Studio update thread.
        //
        // Unity APIs are NOT thread-safe, and SetValue invokes OnValueChanged which may cascade
        // into Unity-dependent game logic. For that reason, prefer calling SetValue from the main
        // thread. The WarnIfOffMainThread helper is retained as an opt-in diagnostic.

        private int _mainThreadId;

        protected virtual void OnEnable()
        {
            _mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        protected void WarnIfOffMainThread(string assetName)
        {
            if (System.Threading.Thread.CurrentThread.ManagedThreadId != _mainThreadId)
            {
                Debug.LogWarning($"[UnifyAudio] SetValue called off main thread on '{assetName}'. " +
                                 "FMOD Studio API handles threading internally, but Unity " +
                                 "APIs and OnValueChanged subscribers require the main thread.");
            }
        }
    }
}