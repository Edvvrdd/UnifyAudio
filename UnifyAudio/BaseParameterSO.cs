using UnityEngine;

namespace UnifyAudio.Parameters
{
    public abstract class UnifyParameterAsset : ScriptableObject
    {
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
                                 "FMOD Studio API is not thread safe.");
            }
        }
    }
}