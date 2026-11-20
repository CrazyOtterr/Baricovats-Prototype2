using UnityEngine;

namespace Baricovats.Prototype2
{
    public sealed class CampaignBootstrap : MonoBehaviour
    {
        public GameBalanceConfig config;
        public bool disablePrototype1Managers = true;

        private void Awake()
        {
            var stateManager = GetComponent<GameStateManager>();
            if (stateManager == null) stateManager = gameObject.AddComponent<GameStateManager>();
            stateManager.config = config;
            if (GetComponent<DayCycleManager>() == null) gameObject.AddComponent<DayCycleManager>();
            if (GetComponent<BuildingManager>() == null) gameObject.AddComponent<BuildingManager>();
            if (GetComponent<FactionManager>() == null) gameObject.AddComponent<FactionManager>();
            if (disablePrototype1Managers) DisableLegacyManagers();
        }

        private void DisableLegacyManagers()
        {
            var legacy = GetComponents<MonoBehaviour>();
            foreach (var component in legacy)
            {
                if (component == this || component is GameStateManager || component is DayCycleManager) continue;
                if (component.GetType().Name == "GameManager" || component.GetType().Name == "EventManager") component.enabled = false;
            }
        }
    }
}
