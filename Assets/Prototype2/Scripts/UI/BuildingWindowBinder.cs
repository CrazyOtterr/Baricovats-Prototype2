using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Baricovats.Prototype2
{
    public sealed class BuildingWindowBinder : MonoBehaviour
    {
        public TMP_Text title;
        public TMP_Text description;
        public Button actionButtonA;
        public Button actionButtonB;
        public Button closeButton;
        public TMP_Text actionLabelA;
        public TMP_Text actionLabelB;
        private BuildingId building;

        public void Show(BuildingId id)
        {
            building = id;
            gameObject.SetActive(true);
            Configure();
        }

        private void OnEnable()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Close);
                closeButton.onClick.AddListener(Close);
            }
            Configure();
        }

        private void Start()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
        }

        private void Configure()
        {
            if (title == null) return;
            title.text = DisplayName(building);
            if (description != null) description.text = "Выберите действие здания.";
            SetButton(actionButtonA, ActionA(), true);
            SetButton(actionButtonB, ActionB(), ActionB() != BuildingActionId.CallExtraEvent || building == BuildingId.Factory);
            if (actionLabelA != null) actionLabelA.text = ActionLabel(ActionA());
            if (actionLabelB != null) actionLabelB.text = ActionLabel(ActionB());
        }

        private void SetButton(Button button, BuildingActionId action, bool visible)
        {
            if (button == null) return;
            button.gameObject.SetActive(visible);
            button.onClick.RemoveAllListeners();
            if (visible) button.onClick.AddListener(() => Execute(action));
        }

        private void Execute(BuildingActionId action)
        {
            if (BuildingManager.Instance == null)
            {
                Debug.LogError("Prototype2: BuildingManager is missing from the GameManager object.");
                return;
            }
            if (BuildingManager.Instance.TryAction(building, action))
            {
                RefreshBuildingVisual(building);
                Close();
            }
            else
            {
                var state = GameStateManager.Instance != null ? GameStateManager.Instance.State : null;
                var buildingState = state != null ? state.GetBuilding(building) : null;
                if (buildingState != null && buildingState.refusedToday)
                {
                    RefreshBuildingVisual(building);
                    Close();
                }
                else if (description != null)
                {
                    description.text = "Действие сейчас недоступно.";
                }
            }
        }

        private void RefreshBuildingVisual(BuildingId id)
        {
            foreach (var point in FindObjectsByType<CityPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!TryGetBuildingId(point.pointName, out var pointId) || pointId != id)
                    continue;
                point.Resolve();
                break;
            }
        }

        private static bool TryGetBuildingId(string pointName, out BuildingId id)
        {
            switch (pointName)
            {
                case "Administration": id = BuildingId.Administration; return true;
                case "Warehouse": id = BuildingId.Warehouse; return true;
                case "Facility":
                case "Factory": id = BuildingId.Factory; return true;
                case "Boiler":
                case "BoilerHouse":
                case "WaterStation": id = BuildingId.Boiler; return true;
                default: id = default; return false;
            }
        }

        public void Close() { gameObject.SetActive(false); }

        private BuildingActionId ActionA() => building switch { BuildingId.Administration => BuildingActionId.CallExtraEvent, BuildingId.Warehouse => BuildingActionId.RestockFood, BuildingId.Factory => BuildingActionId.ProduceWeapons, _ => BuildingActionId.ActivateBoiler };
        private BuildingActionId ActionB() => building == BuildingId.Factory ? BuildingActionId.ProduceHousehold : BuildingActionId.CallExtraEvent;
        private static string ActionLabel(BuildingActionId action) => action switch { BuildingActionId.CallExtraEvent => "Вызвать событие", BuildingActionId.RestockFood => "Пополнить провизию", BuildingActionId.ProduceWeapons => "Произвести оружие", BuildingActionId.ProduceHousehold => "Произвести бытовые вещи", _ => "Запустить котельную" };
        private static string DisplayName(BuildingId id) => id switch { BuildingId.Administration => "Администрация", BuildingId.Warehouse => "Склад", BuildingId.Factory => "Завод", _ => "Котельная" };
    }
}
