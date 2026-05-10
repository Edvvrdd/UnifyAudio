using UnityEngine;
using FMODUnity;

namespace UnifyAudio
{
    public abstract class UnifyEventCollection : ScriptableObject
    {
        public void PlayOneShot(EventReference eventRef, Vector3 position = default)
        {
            RuntimeManager.PlayOneShot(eventRef, position);
        }

        public void PlayOneShotAttached(EventReference eventRef, GameObject target)
        {
            RuntimeManager.PlayOneShotAttached(eventRef, target);
        }
    }
}