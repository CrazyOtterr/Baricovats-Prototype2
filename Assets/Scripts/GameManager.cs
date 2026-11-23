using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Days")]
    public int currentDay = 1;
    public int maxDays = 3;

    [Header("Resources")]
    public int food = 40;
    public int materials = 30;
    public int meds = 10;
    public int workers = 15;

    public int water = 50;

    [Header("Faction")]
    public int neighborUnionApproval = 70; 

    private bool isGameOver = false;
    [Header("Limits")]
    public int maxBuildingActionsPerDay = 2;
    private int buildingActionsUsedToday = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        UpdateUI();
    }

    public void EndDay()
    {
    if (isGameOver) return;

    Debug.Log($"=== End of Day {currentDay} ===");

    ProcessDailyConsumption();

    CheckCriticalStates();

    TriggerEndDayEvent();

    foreach (var cp in UnityEngine.Object.FindObjectsByType<CityPoint>(UnityEngine.FindObjectsSortMode.None))
    {
        cp.ResetResolved();
    }

        // reset per-day counters
        buildingActionsUsedToday = 0;

    currentDay++;

    if (currentDay > maxDays)
    {
        EndPrototype();
        return;
    }

    UpdateUI();
    }


    void ResolveDailyNeeds()
    {
        // базовое потребление
        food -= 5;
        if (food < 0)
        {
            neighborUnionApproval -= 10;
            food = 0;
        }
    }
    void ProcessDailyConsumption()
    {
    food -= 5;

    if (food < 0)
        {
        food = 0;
        ModifyApproval(-10); // люди недовольны
        Debug.Log("Food shortage! Union displeased.");
        }
    // check for resource depletion after daily consumption
    CheckForResourceDepletion();
    }

    void CheckCriticalStates()
    {
        if (neighborUnionApproval <= 20)
        {
            Debug.Log("Union threatens unrest.");
        }
    }

    void TriggerEndDayEvent()
    {
        if (EventManager.Instance == null) return;

        // Chance to trigger an event at end of day
        // Days: currentDay starts at 1. On day 3 (or greater) event is guaranteed.
        if (currentDay >= 3)
        {
            EventManager.Instance.TriggerRandomEvent();
            return;
        }

        // 50-70% chance (use 60% default)
        float chance = 0.6f;
        if (Random.value < chance)
            EventManager.Instance.TriggerRandomEvent();
    }

    void EndGame(string reason)
    {
        if (isGameOver) return;
        isGameOver = true;
        Debug.Log($"=== GAME OVER: {reason} ===");
        // Disable end-day button to prevent further play
        if (UIManager.Instance != null)
            UIManager.Instance.SetEndDayInteractable(false);
        // Additional game-over handling (UI, scene, etc.) can be added here
    }

    void CheckForResourceDepletion()
    {
        if (isGameOver) return;

        if (food <= 0)
        {
            EndGame("Food depleted");
            return;
        }

        if (materials <= 0)
        {
            EndGame("Materials depleted");
            return;
        }

        if (meds <= 0)
        {
            EndGame("Meds depleted");
            return;
        }

        if (workers <= 0)
        {
            EndGame("Workers depleted");
            return;
        }

        if (water <= 0)
        {
            EndGame("Water depleted");
            return;
        }
    }

    void EndPrototype()
    {
        Debug.Log("=== First 3 days of siege survived ===");
        // позже тут будет финальный экран
    }

    void TriggerEvent()
    {
        EventManager.Instance.TriggerRandomEvent();
    }

    public void ModifyApproval(int value)
    {
        neighborUnionApproval = Mathf.Clamp(neighborUnionApproval + value, 0, 100);
        UpdateUI();
    }

    public void ModifyResource(string type, int value)
    {
        if (isGameOver) return;
        switch (type)
        {
            case "food": food += value; break;
            case "water": water += value; break;
            case "materials": materials += value; break;
            case "meds": meds += value; break;
            case "workers": workers += value; break;
        }
        UpdateUI();
        CheckForResourceDepletion();
    }

    // Try to consume one building action for this day. Returns true if allowed.
    public bool UseBuildingAction()
    {
        if (isGameOver) return false;
        if (buildingActionsUsedToday >= maxBuildingActionsPerDay)
            return false;
        buildingActionsUsedToday++;
        return true;
    }

    // Returns true if player still has building actions available this day
    public bool HasBuildingActionAvailable()
    {
        if (isGameOver) return false;
        return buildingActionsUsedToday < maxBuildingActionsPerDay;
    }
        public void AssignWorkersToPoint(string pointName)
    {
        if (workers <= 0)
        {
            Debug.Log("No workers available!");
            return;
        }

            workers--;
            CheckForResourceDepletion();

        switch (pointName)
        {
            case "Warehouse":
                materials += 5;
                Debug.Log("Workers sent to Warehouse");
                break;

            case "WaterStation":
                food += 3;
                ModifyApproval(+5);
                Debug.Log("Water stabilized");
                break;

            case "Administration":
                ModifyApproval(+3);
                Debug.Log("Administration calmed people");
                break;
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        UIManager.Instance.UpdateDay(currentDay);
        UIManager.Instance.UpdateResources(food, materials, meds, workers, water);
        UIManager.Instance.UpdateNeighborApproval(neighborUnionApproval);
    }
}
