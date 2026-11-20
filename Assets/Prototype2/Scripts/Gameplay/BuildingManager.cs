using UnityEngine;

namespace Baricovats.Prototype2
{
    public sealed class BuildingManager : MonoBehaviour
    {
        public static BuildingManager Instance { get; private set; }
        private GameStateManager stateManager;
        private DayCycleManager dayCycle;

        private void Awake() { Instance = this; stateManager = GetComponent<GameStateManager>(); dayCycle = GetComponent<DayCycleManager>(); }

        public bool TryAction(BuildingId id, BuildingActionId action)
        {
            if (!EnsureDependencies())
                return false;

            if (action == BuildingActionId.ActivateBoiler && stateManager != null && stateManager.State != null && stateManager.State.fuel < stateManager.config.boilerFuelCost) return false;
            if (!dayCycle.TryUseAction(id)) return false;
            var state = stateManager.State;
            var config = stateManager.config;

            if (ShouldRefuse(state, id))
            {
                var building = state.GetBuilding(id);
                if (building != null)
                {
                    building.refusedToday = true;
                    state.lastStatus = $"«{building.displayName}» отказалось выполнять действие.";
                }
                stateManager.NotifyChanged();
                return false;
            }

            switch (action)
            {
                case BuildingActionId.RestockFood:
                    int foodProduction = config.warehouseFoodProduction;
                    var civil = state.GetFaction(FactionId.CivilianCouncil);
                    if (civil != null && civil.reputation >= config.highReputationThreshold)
                        foodProduction += config.civilWarehouseFoodBonus;
                    state.food += foodProduction;
                    break;
                case BuildingActionId.ProduceWeapons: state.weapons += config.factoryWeaponsProduction; break;
                case BuildingActionId.ProduceHousehold: state.householdGoods += config.factoryHouseholdProduction; break;
                case BuildingActionId.ActivateBoiler:
                    if (state.fuel < config.boilerFuelCost) return false;
                    state.fuel -= config.boilerFuelCost;
                    state.heatingAndWaterAvailable = true;
                    state.boilerActivatedToday = true;
                    state.daysSinceBoilerActivation = 0;
                    break;
                case BuildingActionId.CallExtraEvent: state.extraEventQueued = true; break;
            }
            stateManager.ClampResources();
            return true;
        }

        private bool EnsureDependencies()
        {
            if (stateManager == null)
                stateManager = GetComponent<GameStateManager>();
            if (dayCycle == null)
                dayCycle = GetComponent<DayCycleManager>();

            if (stateManager == null || dayCycle == null)
            {
                Debug.LogError("Prototype2: GameStateManager or DayCycleManager is missing from the GameManager object.");
                return false;
            }

            if (stateManager.State == null)
                stateManager.ResetCampaign();

            if (stateManager.config == null || stateManager.State == null)
            {
                Debug.LogError("Prototype2: GameBalanceConfig is not assigned or campaign state could not be initialized.");
                return false;
            }

            return true;
        }

        private bool ShouldRefuse(CityState state, BuildingId buildingId)
        {
            if (buildingId == BuildingId.Administration)
                return false;

            FactionId factionId;
            switch (buildingId)
            {
                case BuildingId.Warehouse: factionId = FactionId.CivilianCouncil; break;
                case BuildingId.Factory: factionId = FactionId.DefenseCommittee; break;
                case BuildingId.Boiler: factionId = FactionId.NeighborUnion; break;
                default: return false;
            }

            var faction = state.GetFaction(factionId);
            if (faction == null || faction.reputation >= stateManager.config.lowReputationThreshold)
                return false;

            return Random.Range(0, 100) < stateManager.config.buildingRefusalChance;
        }
    }
}
