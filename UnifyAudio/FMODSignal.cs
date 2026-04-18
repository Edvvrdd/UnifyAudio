using System;
using UnityEngine;

namespace UnifyAudio
{
    [CreateAssetMenu(menuName = "UnifyAudio/Signal")]
    public class FMODSignal : ScriptableObject
    {
        public event Action OnFired;

        public void Fire()
        {
            if (OnFired == null)
            {
                Debug.LogWarning($"[UnifyAudio] Signal '{name}' was fired but nothing is listening. " +
                                 "Ensure at least one FMODEventController has this signal slotted.");
                return;
            }

            OnFired.Invoke();
        }

        private void OnDisable()
        {
            OnFired = null;
        }
    }
}