using UnifyAudio.Parameters;

namespace UnifyAudio
{
    [System.Serializable]
    public class UnifyParameterBinding
    {
        public string ParameterName;
        public UnifyParameter Value;
        public float Min;
        public float Max;
        public float DefaultValue;
        public ParameterType ParameterType;
        public string[] Labels;
    }
}