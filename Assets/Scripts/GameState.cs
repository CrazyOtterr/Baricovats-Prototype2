using UnityEngine;

[System.Serializable]
public class GameState
{
    public int day = 1;
    public int maxDays = 3;

    public int food = 40;
    public int materials = 30;
    public int meds = 10;

    public int water = 50;

    public int workers = 15;

    public int neighborUnionApproval = 70;
}
