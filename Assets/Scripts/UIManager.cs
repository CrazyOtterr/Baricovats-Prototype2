using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Top Panel")]
    public TMP_Text dayText;
    public TMP_Text foodText;
    public TMP_Text materialsText;
    public TMP_Text medsText;
    public TMP_Text workersText;
    public TMP_Text neighborApprovalText;
    public TMP_Text waterText;
    public Button endDayButton;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateDay(int day)
    {
        dayText.text = $"День {day}";
    }

    public void UpdateResources(int food, int mat, int meds, int workers, int water)
    {
        foodText.text = $"{food}";
        materialsText.text = $"{mat}";
        medsText.text = $"{meds}";
        workersText.text = $"{workers}";
        waterText.text = $"{water}";
    }

    public void UpdateNeighborApproval(int value)
    {
        neighborApprovalText.text = $"Поддержка: {value}%";
    }

    public void SetEndDayInteractable(bool value)
    {
        if (endDayButton != null)
            endDayButton.interactable = value;
    }
}
