using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Baricovats.Prototype2
{
    public sealed class FactionReputationBinder : MonoBehaviour
    {
        [System.Serializable] public sealed class FactionColumn { public Slider bar; public TMP_Text valueText; }
        public FactionColumn civilianCouncil;
        public FactionColumn defenseCommittee;
        public FactionColumn neighborUnion;

        private void OnEnable() { Subscribe(); Refresh(GameStateManager.Instance != null ? GameStateManager.Instance.State : null); }
        private void Start() { Subscribe(); Refresh(GameStateManager.Instance != null ? GameStateManager.Instance.State : null); }
        private void OnDisable() { if (GameStateManager.Instance != null) GameStateManager.Instance.StateChanged -= Refresh; }
        private void Subscribe() { if (GameStateManager.Instance != null) GameStateManager.Instance.StateChanged -= Refresh; if (GameStateManager.Instance != null) GameStateManager.Instance.StateChanged += Refresh; }
        private void Refresh(CityState state)
        {
            if (state == null) return;
            RefreshColumn(civilianCouncil, state.GetReputation(FactionId.CivilianCouncil));
            RefreshColumn(defenseCommittee, state.GetReputation(FactionId.DefenseCommittee));
            RefreshColumn(neighborUnion, state.GetReputation(FactionId.NeighborUnion));
        }
        private static void RefreshColumn(FactionColumn column, int value)
        {
            if (column == null) return;
            if (column.bar != null) { column.bar.minValue = 0; column.bar.maxValue = 100; column.bar.value = value; }
            if (column.valueText != null) column.valueText.text = value.ToString();
        }
    }
}
