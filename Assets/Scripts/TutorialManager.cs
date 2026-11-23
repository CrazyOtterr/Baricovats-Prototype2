using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Tooltip("List of contextual tips to use in the game")] 
    public TutorialTip[] tips;

    [Tooltip("Prefab for the tip UI. Must contain a TipUI component.")]
    public GameObject tipUIPrefab;

    private GameObject currentTipGO;
    private TipUI currentTipUI;
    private readonly HashSet<string> shownIds = new HashSet<string>();

    private const string PrefKeyPrefix = "TutorialShown:";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadShownIds();
    }

    private void Start()
    {
        // Try show tips marked showOnStart
        foreach (var t in tips)
        {
            if (t != null && t.showOnStart)
                TryShowTip(t.id);
        }
    }

    void LoadShownIds()
    {
        shownIds.Clear();
        foreach (var t in tips)
        {
            if (t == null) continue;
            if (PlayerPrefs.GetInt(PrefKeyPrefix + t.id, 0) == 1)
                shownIds.Add(t.id);
        }
    }

    bool IsShown(string id) => shownIds.Contains(id);

    void MarkShown(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        shownIds.Add(id);
        PlayerPrefs.SetInt(PrefKeyPrefix + id, 1);
        PlayerPrefs.Save();
    }

    public void TryShowTip(string id)
    {
        if (string.IsNullOrEmpty(id)) return;

        var tip = System.Array.Find(tips, x => x != null && x.id == id);
        if (tip == null) return;

        if (tip.showOnce && IsShown(tip.id)) return;

        ShowTip(tip);
    }

    public void ShowTip(TutorialTip tip)
    {
        if (tip == null) return;
        StartCoroutine(ShowTipCoroutine(tip));
    }

    IEnumerator ShowTipCoroutine(TutorialTip tip)
    {
        if (tip.delay > 0f) yield return new WaitForSeconds(tip.delay);

        // If there is already a tip shown, hide it first
        DismissCurrentTip();

        if (tipUIPrefab == null)
        {
            Debug.LogWarning("TutorialManager: tipUIPrefab not assigned.");
            yield break;
        }

        currentTipGO = Instantiate(tipUIPrefab, Vector3.zero, Quaternion.identity);
        currentTipUI = currentTipGO.GetComponent<TipUI>();
        if (currentTipUI == null)
        {
            Debug.LogWarning("TutorialManager: tipUIPrefab must contain a TipUI component.");
            Destroy(currentTipGO);
            currentTipGO = null;
            yield break;
        }

        currentTipUI.Initialize(tip, OnTipDismissed);

        // Highlight target if it has HighlightTarget
        if (tip.target != null)
        {
            var h = tip.target.GetComponent<HighlightTarget>();
            if (h != null) h.SetHighlighted(true);
        }

        if (tip.showOnce) MarkShown(tip.id);
    }

    void OnTipDismissed()
    {
        DismissCurrentTip();
    }

    void DismissCurrentTip()
    {
        if (currentTipUI != null)
        {
            // clear highlight
            var tip = currentTipUI.CurrentTip;
            if (tip != null && tip.target != null)
            {
                var h = tip.target.GetComponent<HighlightTarget>();
                if (h != null) h.SetHighlighted(false);
            }
        }

        if (currentTipGO != null)
        {
            Destroy(currentTipGO);
            currentTipGO = null;
            currentTipUI = null;
        }
    }

    // Public: hide any tip currently shown
    public void HideTip()
    {
        DismissCurrentTip();
    }
}
