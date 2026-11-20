using System;
using UnityEngine;

namespace Baricovats.Prototype2
{
    public sealed class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }
        public GameBalanceConfig config;
        public CityState State { get; private set; }
        public event Action<CityState> StateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (config != null) ResetCampaign();
        }

        private void Start() { ResetCampaign(); }

        public void ResetCampaign()
        {
            if (config == null) { Debug.LogError("Prototype2: GameBalanceConfig is not assigned."); return; }
            State = config.CreateInitialState();
            StateChanged?.Invoke(State);
        }

        public void ClampResources()
        {
            if (State == null) return;
            State.food = Mathf.Clamp(State.food, 0, 100);
            State.fuel = Mathf.Clamp(State.fuel, 0, 100);
            State.weapons = Mathf.Clamp(State.weapons, 0, 100);
            State.householdGoods = Mathf.Clamp(State.householdGoods, 0, 100);
            State.frontStrength = Mathf.Clamp(State.frontStrength, 0, 100);
            foreach (var faction in State.factions) faction.reputation = Mathf.Clamp(faction.reputation, 0, 100);
            StateChanged?.Invoke(State);
        }

        public void NotifyChanged() => StateChanged?.Invoke(State);

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
                RestartCampaign();
        }

        public void RestartCampaign()
        {
            ResetCampaign();
            if (Prototype2EventManager.Instance != null)
                Prototype2EventManager.Instance.ResetCampaign();
            if (DayCycleManager.Instance != null)
                DayCycleManager.Instance.BeginDay();
        }
    }
}
