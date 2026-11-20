using System;
using System.Collections.Generic;

namespace Baricovats.Prototype2
{
    [Serializable]
    public class CityState
    {
        public int day = 1;
        public int maxDays = 9;
        public int food = 50;
        public int fuel = 50;
        public int weapons = 50;
        public int householdGoods = 50;
        public int frontStrength = 100;
        public bool heatingAndWaterAvailable;
        public int daysSinceBoilerActivation;
        public bool boilerActivatedToday;
        public bool extraEventQueued;
        public bool mandatoryEventQueued;
        public bool isNightTransition;
        public string activeDelayedEffectId = "";
        public bool mapActionsLocked;
        public bool campaignEnded;
        public EndingId ending = EndingId.None;
        public string endingReason = "";
        public int actionsUsedToday;
        public int maxActionsPerDay = 2;
        public string lastStatus = "";

        public List<FactionState> factions = new List<FactionState>();
        public List<BuildingState> buildings = new List<BuildingState>();
        public List<string> flags = new List<string>();
        public List<DelayedEffect> delayedEffects = new List<DelayedEffect>();
        public List<BuildingId> usedBuildingIds = new List<BuildingId>();

        public FactionState GetFaction(FactionId id)
        {
            for (int i = 0; i < factions.Count; i++)
            {
                if (factions[i].id == id)
                    return factions[i];
            }

            return null;
        }

        public BuildingState GetBuilding(BuildingId id)
        {
            for (int i = 0; i < buildings.Count; i++)
            {
                if (buildings[i].id == id)
                    return buildings[i];
            }

            return null;
        }

        public int GetReputation(FactionId id)
        {
            var faction = GetFaction(id);
            return faction != null ? faction.reputation : 0;
        }

        public bool HasFlag(string flag)
        {
            return flags.Contains(flag);
        }

        public void AddFlag(string flag)
        {
            if (string.IsNullOrEmpty(flag) || flags.Contains(flag))
                return;
            flags.Add(flag);
        }

        public void RemoveFlag(string flag)
        {
            flags.Remove(flag);
        }
    }
}
