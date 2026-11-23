using System;

namespace Baricovats.Prototype2
{
    [Serializable]
    public class DelayedEffect
    {
        public string id;
        public string sourceEventId;
        public int triggerDay;
        public string followUpEventId;
        public bool isResolved;
        public int deferralCount;
    }
}
