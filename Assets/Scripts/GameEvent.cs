using UnityEngine;

[System.Serializable]
public class GameEvent
{
    public string title;
    [TextArea]
    public string body;
    public EventChoice[] choices;

    public GameEvent(string title, string body, params EventChoice[] choices)
    {
        this.title = title;
        this.body = body;
        this.choices = choices;
    }
}

