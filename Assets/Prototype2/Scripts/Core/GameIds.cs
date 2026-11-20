namespace Baricovats.Prototype2
{
    public enum FactionId
    {
        CivilianCouncil = 0,
        DefenseCommittee = 1,
        NeighborUnion = 2
    }

    public enum BuildingId
    {
        Administration = 0,
        Warehouse = 1,
        Factory = 2,
        Boiler = 3
    }

    public enum BuildingActionId
    {
        CallExtraEvent,
        RestockFood,
        ProduceWeapons,
        ProduceHousehold,
        ActivateBoiler
    }

    public enum DayPhase
    {
        Idle,
        StartDay,
        Artillery,
        MapActions,
        ExtraEvent,
        MandatoryEvent,
        Consumption,
        Checks,
        Night,
        Ended
    }

    public enum EndingId
    {
        None = 0,
        Defeat = 1,
        DefenseCommittee = 2,
        CivilianCouncil = 3,
        NeighborUnion = 4,
        LowReputationVictory = 5,
        MidReputationVictory = 6
    }
}
