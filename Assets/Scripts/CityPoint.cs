using UnityEngine;
using UnityEngine.EventSystems;

public class CityPoint : MonoBehaviour, IPointerClickHandler
{
    public string pointName;

    private bool isResolved = false;
    private SpriteRenderer spriteRenderer;
    [Tooltip("Optional existing GameObject used to mark building damage.")]
    public GameObject damageOverlay;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (Baricovats.Prototype2.GameStateManager.Instance != null)
        {
            Baricovats.Prototype2.GameStateManager.Instance.StateChanged += RefreshPrototype2Visual;
            RefreshPrototype2Visual(Baricovats.Prototype2.GameStateManager.Instance.State);
        }
    }

    private void OnDisable()
    {
        if (Baricovats.Prototype2.GameStateManager.Instance != null)
            Baricovats.Prototype2.GameStateManager.Instance.StateChanged -= RefreshPrototype2Visual;
    }

    private void Start()
    {
        if (Baricovats.Prototype2.GameStateManager.Instance != null)
            RefreshPrototype2Visual(Baricovats.Prototype2.GameStateManager.Instance.State);
    }

    private void RefreshPrototype2Visual(Baricovats.Prototype2.CityState state)
    {
        if (state == null || spriteRenderer == null)
            return;
        if (!TryGetPrototype2Building(out var buildingId))
            return;

        var building = state.GetBuilding(buildingId);
        if (building == null)
            return;

        bool unavailable = building.usedToday || building.refusedToday || building.isDamaged
            || state.day < building.disabledUntilDay;
        isResolved = unavailable;
        spriteRenderer.color = unavailable ? Color.gray : Color.white;
        if (damageOverlay != null)
            damageOverlay.SetActive(building.isDamaged);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isResolved) return;

        if (Baricovats.Prototype2.GameStateManager.Instance != null)
        {
            var stateManager = Baricovats.Prototype2.GameStateManager.Instance;
            var state = stateManager.State;
            if (state == null || state.campaignEnded || state.mapActionsLocked)
                return;
            if (state.actionsUsedToday >= state.maxActionsPerDay)
                return;
            if (TryGetPrototype2Building(out var buildingId))
            {
                var building = state.GetBuilding(buildingId);
                if (building == null || !building.IsInteractable(state.day))
                    return;

                var binder = FindObjectsByType<Baricovats.Prototype2.BuildingWindowBinder>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
                if (binder.Length > 0)
                {
                    binder[0].Show(buildingId);
                    return;
                }

                Debug.LogError("Prototype2: BuildingWindowBinder is not present in the scene.");
            }
            return;
        }

        // Block opening building window if player used all allowed building actions today
        if (GameManager.Instance != null && !GameManager.Instance.HasBuildingActionAvailable())
        {
            Debug.Log("No building actions left for today.");
            return;
        }

        OpenWindow();
    }

    void OpenWindow()
    {
        // Prototype 2 uses the editor-bound window and its own state managers.
        // Keep the Prototype 1 path below as a fallback for the old scene.
        if (Baricovats.Prototype2.GameStateManager.Instance != null)
        {
            var binders = FindObjectsByType<Baricovats.Prototype2.BuildingWindowBinder>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var binder = binders.Length > 0 ? binders[0] : null;
            if (binder != null && TryGetPrototype2Building(out var buildingId))
            {
                binder.Show(buildingId);
                return;
            }

            Debug.LogError("Prototype2: BuildingWindowBinder is not present in the scene.");
            return;
        }

        BuildingData data = null;

        switch (pointName)
        {
            case "Warehouse":
                data = new BuildingData(
                    "Склад",
                    "Хранит продовольствие и материалы.",
                    new BuildingAction("Отправить рабочих", () =>
                    {
                        GameManager.Instance.AssignWorkersToPoint("Warehouse");
                    })
                );
                break;

            case "WaterStation":
                data = new BuildingData(
                    "Водяная станция",
                    "Обеспечивает город водой.",
                    new BuildingAction("Починить", () =>
                    {
                        GameManager.Instance.ModifyResource("materials", -5);
                        GameManager.Instance.ModifyApproval(+5);
                    })
                );
                break;
            
            case "Administration":
                data = new BuildingData(
                    "Администрация",
                    "Орган управления городом.",
                    new BuildingAction("Посетить администрацию", () =>
                    {
                        GameManager.Instance.ModifyApproval(+3);
                    })
                );
                break;

            case "Facility":
                data = new BuildingData(
                    "Завод",
                    "Производит технику для фронта.",
                    new BuildingAction("Производить", () =>
                    {
                        GameManager.Instance.ModifyResource("materials", +3);
                        GameManager.Instance.ModifyApproval(+1);
                    })
                );
                break;
        }   

        if (data != null)
            BuildingWindowUI.Instance.Show(data, this);
    }

    private bool TryGetPrototype2Building(out Baricovats.Prototype2.BuildingId id)
    {
        switch (pointName)
        {
            case "Administration": id = Baricovats.Prototype2.BuildingId.Administration; return true;
            case "Warehouse": id = Baricovats.Prototype2.BuildingId.Warehouse; return true;
            case "Facility":
            case "Factory": id = Baricovats.Prototype2.BuildingId.Factory; return true;
            case "Boiler":
            case "BoilerHouse": id = Baricovats.Prototype2.BuildingId.Boiler; return true;
            default: id = default; return false;
        }
    }

    public void Resolve()
    {
        isResolved = true;

        GetComponent<SpriteRenderer>().color = Color.gray;
    }

    // Reset resolved state (called at end of day)
    public void ResetResolved()
    {
        isResolved = false;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = Color.white;
    }
}
