using UnityEngine;

[System.Serializable]
public class TutorialTip
{
    public string id;
    public string title;
    [TextArea]
    public string text;

    // Target object in the scene / UI to point to
    public GameObject target;

    // If true, tip will be shown only once (saved to PlayerPrefs)
    public bool showOnce = true;

    // If true, try to show this tip on Start()
    public bool showOnStart = false;

    // Optional delay before showing (seconds)
    public float delay = 0f;
}
