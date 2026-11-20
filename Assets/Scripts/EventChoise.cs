using UnityEngine;
using System;

[Serializable]
public class EventChoice
{
    public string text;

    // Numerical effects (used by simple prototype model)
    public int approvalChange;
    public int foodChange;
    public int waterChange;
    public int materialsChange;
    public int medsChange;
    public int workersChange;

    // Chance to trigger a secondary/random event (0..1)
    public float probabilityOfSecondary = 0f;

    // Optional custom action for complex effects
    public Action action;

    public EventChoice(string text, Action action)
    {
        this.text = text;
        this.action = action;
    }

    public EventChoice(string text, int approval = 0, int food = 0, int water = 0, int materials = 0, int meds = 0, int workers = 0, float probabilityOfSecondary = 0f, Action action = null)
    {
        this.text = text;
        this.approvalChange = approval;
        this.foodChange = food;
        this.waterChange = water;
        this.materialsChange = materials;
        this.medsChange = meds;
        this.workersChange = workers;
        this.probabilityOfSecondary = probabilityOfSecondary;
        this.action = action;
    }

    public void Apply()
    {
        // Apply numeric effects through GameManager if available
        if (GameManager.Instance != null)
        {
            if (approvalChange != 0)
                GameManager.Instance.ModifyApproval(approvalChange);

            if (foodChange != 0)
                GameManager.Instance.ModifyResource("food", foodChange);

            if (waterChange != 0)
                GameManager.Instance.ModifyResource("water", waterChange);

            if (materialsChange != 0)
                GameManager.Instance.ModifyResource("materials", materialsChange);

            if (medsChange != 0)
                GameManager.Instance.ModifyResource("meds", medsChange);

            if (workersChange != 0)
                GameManager.Instance.ModifyResource("workers", workersChange);
        }

        // Chance to trigger a secondary random event
        if (probabilityOfSecondary > 0f && EventManager.Instance != null)
        {
            if (UnityEngine.Random.value < probabilityOfSecondary)
                EventManager.Instance.TriggerRandomEvent();
        }

        // Finally call custom action if provided
        action?.Invoke();
    }
}

