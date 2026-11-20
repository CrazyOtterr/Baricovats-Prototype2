using UnityEngine;

namespace Baricovats.Prototype2
{
    public sealed class FactionManager : MonoBehaviour
    {
        private GameStateManager stateManager;
        private void Awake() { stateManager = GetComponent<GameStateManager>(); }

        public void ChangeReputation(FactionId id, int delta)
        {
            var faction = stateManager != null && stateManager.State != null ? stateManager.State.GetFaction(id) : null;
            if (faction == null) return;
            faction.reputation = Mathf.Clamp(faction.reputation + delta, 0, 100);
            stateManager.NotifyChanged();
        }
    }
}
