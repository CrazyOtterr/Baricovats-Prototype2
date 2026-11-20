using UnityEngine;

namespace Baricovats.Prototype2
{
    public sealed class DayCycleManager : MonoBehaviour
    {
        public static DayCycleManager Instance { get; private set; }
        public DayPhase phase { get; private set; } = DayPhase.Idle;

        private GameStateManager stateManager;
        private GameBalanceConfig Config => stateManager != null ? stateManager.config : null;

        private void Awake() { Instance = this; stateManager = GetComponent<GameStateManager>(); }
        private void Start() { BeginDay(); }

        public void BeginDay()
        {
            if (stateManager == null) return;
            if (stateManager.State == null) stateManager.ResetCampaign();
            if (stateManager.State == null || stateManager.State.campaignEnded) return;
            var state = stateManager.State;
            state.actionsUsedToday = 0;
            state.usedBuildingIds.Clear();
            foreach (var building in state.buildings)
            {
                building.usedToday = false;
                building.refusedToday = false;
                if (building.disabledUntilDay <= state.day)
                {
                    building.disabledUntilDay = 0;
                    building.isDamaged = false;
                }
            }
            state.boilerActivatedToday = false;
            state.mapActionsLocked = false;
            state.isNightTransition = false;
            phase = IsArtilleryDay(state.day) ? DayPhase.Artillery : DayPhase.MapActions;
            if (phase == DayPhase.Artillery)
                ResolveArtillery();
            ProcessDelayedEffects(state);
            stateManager.NotifyChanged();
        }

        public void ResolveArtillery()
        {
            if (phase != DayPhase.Artillery)
                return;

            var state = stateManager.State;
            if (state == null)
            {
                phase = DayPhase.MapActions;
                return;
            }

            if (Config == null)
            {
                phase = DayPhase.MapActions;
                Debug.LogError("Prototype2: GameBalanceConfig is missing during artillery resolution.");
                return;
            }

            var candidates = new System.Collections.Generic.List<BuildingState>();
            var warehouse = state.GetBuilding(BuildingId.Warehouse);
            var factory = state.GetBuilding(BuildingId.Factory);
            var boiler = state.GetBuilding(BuildingId.Boiler);

            if (warehouse != null) { warehouse.damageWeight = Config.warehouseDamageWeight; candidates.Add(warehouse); }
            if (factory != null) { factory.damageWeight = Config.factoryDamageWeight; candidates.Add(factory); }
            if (boiler != null) { boiler.damageWeight = Config.boilerDamageWeight; candidates.Add(boiler); }

            if (candidates.Count == 0)
            {
                state.lastStatus = "";
                phase = DayPhase.MapActions;
                return;
            }

            float totalWeight = 0f;
            foreach (var candidate in candidates)
                totalWeight += Mathf.Max(0.01f, candidate.damageWeight);

            float roll = Random.Range(0f, totalWeight);
            BuildingState target = candidates[candidates.Count - 1];
            foreach (var candidate in candidates)
            {
                roll -= Mathf.Max(0.01f, candidate.damageWeight);
                if (roll <= 0f)
                {
                    target = candidate;
                    break;
                }
            }

            target.isDamaged = true;
            int additionalDisabledDays = Mathf.Max(0, Config.artilleryDisabledDaysAfterHit);
            target.disabledUntilDay = state.day + additionalDisabledDays;
            state.lastStatus = $"Обстрел: повреждено здание «{target.displayName}».";
            phase = DayPhase.MapActions;
            stateManager.NotifyChanged();
        }

        public bool TryUseAction(BuildingId id)
        {
            if (stateManager == null)
                stateManager = GetComponent<GameStateManager>();

            if (stateManager == null)
            {
                Debug.LogError("Prototype2: GameStateManager is missing from the DayCycleManager object.");
                return false;
            }

            var state = stateManager.State;
            if (state == null)
            {
                Debug.LogError("Prototype2: campaign state is not initialized before building action.");
                return false;
            }

            if (phase == DayPhase.Artillery)
                ResolveArtillery();

            if (phase != DayPhase.MapActions || state.actionsUsedToday >= state.maxActionsPerDay) return false;
            var building = state.GetBuilding(id);
            if (building == null || !building.IsInteractable(state.day)) return false;
            building.usedToday = true;
            state.usedBuildingIds.Add(id);
            state.actionsUsedToday++;
            stateManager.NotifyChanged();
            return true;
        }

        public void EndDay()
        {
            if (stateManager == null)
                stateManager = GetComponent<GameStateManager>();
            if (stateManager == null || stateManager.State == null)
                return;

            if (phase == DayPhase.Artillery)
                ResolveArtillery();
            if (phase != DayPhase.MapActions)
                return;

            var state = stateManager.State;

            if (state.extraEventQueued)
            {
                state.extraEventQueued = false;
                phase = DayPhase.ExtraEvent;
                stateManager.NotifyChanged();
                if (Prototype2EventManager.Instance != null)
                    Prototype2EventManager.Instance.ShowExtraEvent();
                return;
            }

            StartScheduledEvent();
        }

        public void OnEventResolved(Prototype2EventData resolvedEvent)
        {
            var state = stateManager.State;
            if (state == null)
                return;

            if (phase == DayPhase.ExtraEvent)
            {
                ResolveActiveDelayedEffect(state);
                StartScheduledEvent();
                return;
            }

            ApplyConsumptionAndChecks();
        }

        private void StartScheduledEvent()
        {
            var state = stateManager.State;
            if (state == null)
                return;

            if (Prototype2EventManager.Instance != null)
            {
                if (state.day == 1)
                {
                    phase = DayPhase.MandatoryEvent;
                    Prototype2EventManager.Instance.ShowMandatoryEvent();
                    return;
                }
                if (state.day == 9)
                {
                    phase = DayPhase.MandatoryEvent;
                    Prototype2EventManager.Instance.ShowMandatoryEvent();
                    return;
                }

                phase = DayPhase.MandatoryEvent;
                Prototype2EventManager.Instance.ShowMainEvent();
                if (Prototype2EventManager.Instance.activeEvent != null)
                    return;
            }

            ApplyConsumptionAndChecks();
        }

        private void ApplyConsumptionAndChecks()
        {
            if (stateManager.State == null)
                return;
            phase = DayPhase.Consumption;
            var state = stateManager.State;
            state.lastStatus = "";

            state.food -= Config.dailyFoodConsumption;
            state.householdGoods -= Config.dailyHouseholdConsumption;

            if (state.heatingAndWaterAvailable)
            {
                if (!state.boilerActivatedToday)
                    state.fuel -= Config.boilerFuelCost;
            }

            if (state.day % 3 == 0)
            {
                int weaponsRequired = GetWeaponsRequirement(state);
                if (state.weapons >= weaponsRequired)
                {
                    state.weapons -= weaponsRequired;
                }
                else
                {
                    state.frontStrength -= Config.frontLossWhenWeaponsShort;
                    state.lastStatus = "Недостаточно оружия: фронт потерял позиции.";
                }
            }

            UpdateHeatingState(state);
            stateManager.ClampResources();

            if (HasDefeat(state))
            {
                state.ending = EndingId.Defeat;
                state.campaignEnded = true;
                state.mapActionsLocked = true;
                if (string.IsNullOrEmpty(state.lastStatus))
                    state.lastStatus = "Поражение: критическое состояние города.";
                phase = DayPhase.Ended;
                stateManager.NotifyChanged();
                return;
            }

            phase = DayPhase.Night;
            state.isNightTransition = true;
            stateManager.NotifyChanged();
            StartCoroutine(FinishNightTransition(state));
        }

        private System.Collections.IEnumerator FinishNightTransition(CityState state)
        {
            float seconds = Config != null ? Config.blackScreenSeconds : 1f;
            yield return new WaitForSeconds(seconds);

            state.isNightTransition = false;
            stateManager.NotifyChanged();

            if (state.day >= state.maxDays)
            {
                phase = DayPhase.Ended;
                state.campaignEnded = true;
                state.mapActionsLocked = true;
                state.ending = EndingId.MidReputationVictory;
                stateManager.NotifyChanged();
                yield break;
            }

            state.day++;
            BeginDay();
        }

        private void ProcessDelayedEffects(CityState state)
        {
            if (Prototype2EventManager.Instance == null)
                return;

            for (int i = state.delayedEffects.Count - 1; i >= 0; i--)
            {
                var effect = state.delayedEffects[i];
                if (effect.isResolved || effect.triggerDay > state.day)
                    continue;

                if (Prototype2EventManager.Instance.TryShowFollowUp(effect.followUpEventId))
                {
                    state.activeDelayedEffectId = effect.id;
                    phase = DayPhase.ExtraEvent;
                }
                else
                {
                    effect.isResolved = true;
                }
            }
        }

        private void ResolveActiveDelayedEffect(CityState state)
        {
            if (string.IsNullOrEmpty(state.activeDelayedEffectId))
                return;

            foreach (var effect in state.delayedEffects)
            {
                if (effect.id == state.activeDelayedEffectId)
                {
                    effect.isResolved = true;
                    break;
                }
            }
            state.activeDelayedEffectId = "";
        }

        private int GetWeaponsRequirement(CityState state)
        {
            int requirement = Config.weaponsEveryThreeDays;
            var defense = state.GetFaction(FactionId.DefenseCommittee);
            if (defense != null && defense.reputation >= Config.highReputationThreshold)
                requirement -= Config.defenseWeaponsRequirementReduction;
            return Mathf.Max(0, requirement);
        }

        private void UpdateHeatingState(CityState state)
        {
            if (!state.heatingAndWaterAvailable)
                return;

            var union = state.GetFaction(FactionId.NeighborUnion);
            int reputation = union != null ? union.reputation : 50;
            int allowedDays = reputation <= 20 ? 1
                : reputation <= 40 ? 2
                : reputation <= 60 ? 3
                : reputation <= 80 ? 4
                : 5;

            if (state.boilerActivatedToday)
                state.daysSinceBoilerActivation = 0;
            else
                state.daysSinceBoilerActivation++;

            if (state.daysSinceBoilerActivation >= allowedDays)
                state.heatingAndWaterAvailable = false;
        }

        private static bool HasDefeat(CityState state)
        {
            if (state.frontStrength <= 0)
                return true;
            if (state.food <= 0)
                return true;

            var union = state.GetFaction(FactionId.NeighborUnion);
            if (union == null || union.reputation >= 20)
                return false;

            var civil = state.GetFaction(FactionId.CivilianCouncil);
            var defense = state.GetFaction(FactionId.DefenseCommittee);
            int civilReputation = civil != null ? civil.reputation : 0;
            int defenseReputation = defense != null ? defense.reputation : 0;

            if (civilReputation >= 50 && defenseReputation >= 50)
                return false;
            if (civilReputation >= 80 || defenseReputation >= 80)
                return false;

            return true;
        }

        private static bool IsArtilleryDay(int day) => day == 1 || day == 3 || day == 6 || day == 9;
    }
}
