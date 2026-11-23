using System;

public class BuildingAction
{
    public string label;
    public Action callback;

    public BuildingAction(string label, Action callback)
    {
        this.label = label;
        this.callback = callback;
    }
}

public class BuildingData
{
    public string title;
    public string description;
    public BuildingAction[] actions;

    public BuildingData(string title, string description, params BuildingAction[] actions)
    {
        this.title = title;
        this.description = description;
        this.actions = actions;
    }
}
