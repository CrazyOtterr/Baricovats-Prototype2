using UnityEngine;

// Simple highlight helper: changes SpriteRenderer or Image color when highlighted
public class HighlightTarget : MonoBehaviour
{
    public Color highlightColor = Color.yellow;

    private SpriteRenderer sr;
    private UnityEngine.UI.Image img;
    private Color originalColor;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        img = GetComponent<UnityEngine.UI.Image>();
        if (sr != null) originalColor = sr.color;
        if (img != null) originalColor = img.color;
    }

    public void SetHighlighted(bool value)
    {
        if (sr != null)
        {
            sr.color = value ? highlightColor : originalColor;
            return;
        }

        if (img != null)
        {
            img.color = value ? highlightColor : originalColor;
            return;
        }
    }
}
