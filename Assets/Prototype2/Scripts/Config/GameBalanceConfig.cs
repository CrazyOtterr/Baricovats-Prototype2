using UnityEngine;

namespace Baricovats.Prototype2
{
    [CreateAssetMenu(menuName = "Baricovats/Prototype2/Balance Config", fileName = "GameBalanceConfig")]
    public sealed class GameBalanceConfig : ScriptableObject
    {
        [Header("Campaign")]
        public int maxDays = 9;
        public int maxActionsPerDay = 2;
        [Min(0.1f)] public float blackScreenSeconds = 5f;

        [Header("Initial city state")]
        [Range(0, 100)] public int food = 50;
        [Range(0, 100)] public int fuel = 50;
        [Range(0, 100)] public int weapons = 50;
        [Range(0, 100)] public int householdGoods = 50;
        [Range(0, 100)] public int frontStrength = 100;

        [Header("Daily consumption")]
        [Min(0)] public int dailyFoodConsumption = 5;
        [Min(0)] public int dailyHouseholdConsumption = 3;
        [Min(0)] public int boilerFuelCost = 5;
        [Min(0)] public int weaponsEveryThreeDays = 10;
        [Min(0)] public int frontLossWhenWeaponsShort = 15;

        [Header("Building production")]
        [Min(0)] public int warehouseFoodProduction = 20;
        [Min(0)] public int factoryWeaponsProduction = 15;
        [Min(0)] public int factoryHouseholdProduction = 15;

        [Header("Artillery")]
        [Min(0.01f)] public float boilerDamageWeight = 1.25f;
        [Min(0.01f)] public float warehouseDamageWeight = 1f;
        [Min(0.01f)] public float factoryDamageWeight = 1f;
        [Min(0)] public int artilleryDisabledDaysAfterHit = 1;

        [Header("Faction defaults")]
        [Range(0, 100)] public int startingReputation = 50;
        [Range(0, 100)] public int lowReputationThreshold = 20;
        [Range(0, 100)] public int highReputationThreshold = 80;
        [Range(0, 100)] public int buildingRefusalChance = 50;

        [Header("Faction bonuses")]
        [Min(0)] public int civilWarehouseFoodBonus = 10;
        [Min(0)] public int defenseWeaponsRequirementReduction = 5;

        public CityState CreateInitialState()
        {
            var state = new CityState
            {
                day = 1,
                maxDays = Mathf.Max(1, maxDays),
                maxActionsPerDay = Mathf.Max(1, maxActionsPerDay),
                food = food,
                fuel = fuel,
                weapons = weapons,
                householdGoods = householdGoods,
                frontStrength = frontStrength,
                heatingAndWaterAvailable = false,
                daysSinceBoilerActivation = 0
            };

            state.factions.Add(new FactionState { id = FactionId.CivilianCouncil, displayName = "Гражданский Совет", reputation = startingReputation, lowThreshold = lowReputationThreshold, highThreshold = highReputationThreshold });
            state.factions.Add(new FactionState { id = FactionId.DefenseCommittee, displayName = "Комитет Обороны", reputation = startingReputation, lowThreshold = lowReputationThreshold, highThreshold = highReputationThreshold });
            state.factions.Add(new FactionState { id = FactionId.NeighborUnion, displayName = "Союз Добрососедства", reputation = startingReputation, lowThreshold = lowReputationThreshold, highThreshold = highReputationThreshold });

            state.buildings.Add(new BuildingState { id = BuildingId.Administration, displayName = "Администрация", factionId = FactionId.CivilianCouncil });
            state.buildings.Add(new BuildingState { id = BuildingId.Warehouse, displayName = "Склад", factionId = FactionId.CivilianCouncil });
            state.buildings.Add(new BuildingState { id = BuildingId.Factory, displayName = "Завод", factionId = FactionId.DefenseCommittee });
            state.buildings.Add(new BuildingState { id = BuildingId.Boiler, displayName = "Котельная", factionId = FactionId.NeighborUnion });
            return state;
        }
    }
}
