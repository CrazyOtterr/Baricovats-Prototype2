using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Baricovats.Prototype2
{
    public sealed class Prototype2Hud : MonoBehaviour
    {
        public TMP_Text dayText;
        public TMP_Text foodText;
        public TMP_Text weaponsText;
        public TMP_Text householdText;
        public TMP_Text fuelText;
        public TMP_Text heatingText;
        public TMP_Text frontText;
        public TMP_Text statusText;
        public TMP_Text factionsSummaryText;
        public Button endDayButton;

        private void OnEnable()
        {
            if (GameStateManager.Instance != null) GameStateManager.Instance.StateChanged += Refresh;
            if (endDayButton != null) endDayButton.onClick.AddListener(EndDay);
            Refresh(GameStateManager.Instance != null ? GameStateManager.Instance.State : null);
        }

        private void OnDisable()
        {
            if (GameStateManager.Instance != null) GameStateManager.Instance.StateChanged -= Refresh;
            if (endDayButton != null) endDayButton.onClick.RemoveListener(EndDay);
        }

        private void EndDay() { if (DayCycleManager.Instance != null) DayCycleManager.Instance.EndDay(); }

        private void Refresh(CityState state)
        {
            if (state == null) return;
            if (dayText != null) dayText.text = $"ДЕНЬ {state.day}/{state.maxDays}";
            if (foodText != null) foodText.text = state.food.ToString();
            if (weaponsText != null) weaponsText.text = state.weapons.ToString();
            if (householdText != null) householdText.text = state.householdGoods.ToString();
            if (fuelText != null) fuelText.text = state.fuel.ToString();
            if (heatingText != null) heatingText.text = state.heatingAndWaterAvailable ? "ЕСТЬ" : "НЕТ";
            if (frontText != null) frontText.text = state.frontStrength.ToString();
            if (statusText != null) statusText.text = state.lastStatus;
            if (factionsSummaryText != null) factionsSummaryText.text = $"СОВЕТ {state.GetReputation(FactionId.CivilianCouncil)}   ОБОРОНА {state.GetReputation(FactionId.DefenseCommittee)}   СОЮЗ {state.GetReputation(FactionId.NeighborUnion)}";
            if (endDayButton != null)
            {
                bool canEndDay = DayCycleManager.Instance != null
                    && DayCycleManager.Instance.phase == DayPhase.MapActions;
                endDayButton.interactable = !state.campaignEnded && !state.mapActionsLocked && canEndDay;
            }
        }
    }
}
