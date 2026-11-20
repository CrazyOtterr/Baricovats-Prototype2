using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIEventWindow : MonoBehaviour
{
    public static UIEventWindow Instance;

    public TMP_Text titleText;
    public TMP_Text bodyText;
    public Transform buttonsParent;
    public GameObject buttonPrefab;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void ShowEvent(GameEvent ev)
    {
        if (ev == null)
        {
            Debug.LogWarning("UIEventWindow.ShowEvent called with null GameEvent.");
            return;
        }

        if (titleText == null || bodyText == null || buttonsParent == null || buttonPrefab == null)
        {
            Debug.LogError("UIEventWindow: one or more UI references are not assigned (titleText, bodyText, buttonsParent, buttonPrefab). Check inspector.");
            return;
        }

        try
        {
            titleText.text = ev.title ?? "(no title)";
            bodyText.text = ev.body ?? string.Empty;

            // Disable end-day button while event is active
            if (UIManager.Instance != null)
                UIManager.Instance.SetEndDayInteractable(false);

            foreach (Transform child in buttonsParent)
                Destroy(child.gameObject);

            if (ev.choices != null)
            {
                foreach (var choice in ev.choices)
                {
                    if (choice == null) continue;

                    var btn = Instantiate(buttonPrefab, buttonsParent);

                    var txt = btn.GetComponentInChildren<TMP_Text>();
                    if (txt != null)
                        txt.text = choice.text ?? "";
                    else
                        Debug.LogWarning("UIEventWindow: button prefab missing TMP_Text child.");

                    var uiBtn = btn.GetComponent<Button>();
                    if (uiBtn != null)
                    {
                        uiBtn.onClick.AddListener(() =>
                        {
                            choice.Apply();
                            // Re-enable end-day button when event resolved
                            if (UIManager.Instance != null)
                                UIManager.Instance.SetEndDayInteractable(true);
                            gameObject.SetActive(false);
                        });
                    }
                    else
                    {
                        Debug.LogWarning("UIEventWindow: button prefab missing Button component.");
                    }
                }
            }

            gameObject.SetActive(true);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception in UIEventWindow.ShowEvent: {ex}");
        }
    }
}
