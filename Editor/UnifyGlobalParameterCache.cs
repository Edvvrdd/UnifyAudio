#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using FMODUnity;
using UnifyAudio.Parameters;

namespace UnifyAudio.Editor
{
    [InitializeOnLoad]
    public static class UnifyGlobalParameterCache
    {
        static UnifyGlobalParameterCache()
        {
            UnifyGlobalParameter.EditorCacheDelegate = CacheParameterDescription;
        }

        private static bool CacheParameterDescription(UnifyGlobalParameter param)
        {
            var paramRef = EventManager.Parameters.Find(p => p.Name == param.Parameter);

            if (paramRef != null)
            {
                param.ApplyCachedDescription(
                    paramRef.Min,
                    paramRef.Max,
                    paramRef.Default,
                    (UnifyAudio.Parameters.ParameterType)paramRef.Type,
                    paramRef.Labels
                );
            }
            else
            {
                Debug.LogWarning(
                    $"[UnifyAudio] Could not find global parameter '{param.Parameter}' " +
                    $"on asset '{param.name}'. Ensure your FMOD project is built and banks are imported.");
            }
            return true;
        }
    }
}
#endif
