using System;

namespace Baricovats.Prototype2
{
    [Serializable]
    public class FactionState
    {
        public FactionId id;
        public string displayName;
        public int reputation;
        public int lowThreshold = 20;
        public int highThreshold = 80;

        public bool IsLow => reputation < lowThreshold;
        public bool IsHigh => reputation >= highThreshold;

        public FactionState Clone()
        {
            return (FactionState)MemberwiseClone();
        }
    }
}
