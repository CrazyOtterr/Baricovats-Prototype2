using System;

namespace Baricovats.Prototype2
{
    [Serializable]
    public class BuildingState
    {
        public BuildingId id;
        public string displayName;
        public FactionId factionId;
        public bool usedToday;
        public bool refusedToday;
        public bool isDamaged;
        public int disabledUntilDay;
        public float damageWeight = 1f;

        public bool IsInteractable(int currentDay)
        {
            if (usedToday || refusedToday)
                return false;
            if (isDamaged)
                return false;
            if (currentDay < disabledUntilDay)
                return false;
            return true;
        }

        public BuildingState Clone()
        {
            return (BuildingState)MemberwiseClone();
        }
    }
}
