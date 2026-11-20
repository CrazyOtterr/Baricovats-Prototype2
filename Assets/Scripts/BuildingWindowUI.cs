using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BuildingWindowUI : MonoBehaviour
{
    public static BuildingWindowUI Instance;

    private CityPoint currentPoint;

    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public Transform buttonsParent;
    public GameObject buttonPrefab;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show(BuildingData data)
    {
        titleText.text = data.title;
        descriptionText.text = data.description;

        foreach (Transform c in buttonsParent)
            Destroy(c.gameObject);

        foreach (var action in data.actions)
        {
            var localAction = action;
            var btn = Instantiate(buttonPrefab, buttonsParent);
            var txt = btn.GetComponentInChildren<TMP_Text>();
            if (txt != null) txt.text = localAction.label;
            var uiBtn = btn.GetComponent<Button>();
            if (uiBtn != null)
            {
                uiBtn.onClick.AddListener(() =>
                {
                    // Check per-day building action limit
                    if (GameManager.Instance != null)
                    {
                        if (!GameManager.Instance.UseBuildingAction())
                        {
                            Debug.Log("No building actions left for today.");
                            return;
                        }
                    }

                    // perform the building action
                    localAction.callback?.Invoke();

                    // resolve the current point and close the window
                    if (currentPoint != null)
                        currentPoint.Resolve();

                    Close();
                });
            }
        }

        gameObject.SetActive(true);
    }

   
    public void Show(BuildingData data, CityPoint point)
    {
        currentPoint = point;
        Show(data);
    }

    public void Close()
    {
        currentPoint = null;
        gameObject.SetActive(false);
    }

    public void OnActionButton()
    {
        if (currentPoint == null) return;

        currentPoint.Resolve(); 
        Close();
    }
    public void TriggerClose()
    {
        Close();
    }

    public void TriggerAction()
    {
        // Call the existing handler which has null-checks and closes the window
        OnActionButton();
    }
}
