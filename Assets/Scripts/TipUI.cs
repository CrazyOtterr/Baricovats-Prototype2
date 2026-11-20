using UnityEngine;
using TMPro;

public class TipUI : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text bodyText;
    public RectTransform rootRect;

    [HideInInspector]
    public TutorialTip CurrentTip;

    private System.Action onDismiss;

    private Canvas parentCanvas;

    public void Initialize(TutorialTip tip, System.Action onDismissCallback)
    {
        CurrentTip = tip;
        onDismiss = onDismissCallback;

        if (titleText != null) titleText.text = tip.title ?? "";
        if (bodyText != null) bodyText.text = tip.text ?? "";

        parentCanvas = GetComponentInParent<Canvas>();

        // Make sure we position near target immediately
        Update();
    }

    void Update()
    {
        if (CurrentTip == null || CurrentTip.target == null || rootRect == null) return;

        Camera cam = null;
        if (Camera.main != null) cam = Camera.main;
        Vector3 worldPos = CurrentTip.target.transform.position;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldPos);

        if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, screenPoint, parentCanvas.worldCamera, out pos);
            rootRect.anchoredPosition = pos + new Vector2(0, 80);
        }
        else
        {
            rootRect.position = screenPoint + new Vector2(0, 80);
        }
    }

    // Called by UI button
    public void Dismiss()
    {
        onDismiss?.Invoke();
        Destroy(gameObject);
    }
}
