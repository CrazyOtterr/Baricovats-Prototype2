using System;

namespace Baricovats.Prototype2
{
    public enum EventSwipeDirection { Left, Right, Down }

    [Serializable]
    public sealed class Prototype2EventData
    {
        public string id;
        public string source = "Администрация";
        public string title;
        public string body;
        public string leftChoice = "ПРИНЯТЬ";
        public string rightChoice = "ОТКЛОНИТЬ";
        public string downChoice = "ОТЛОЖИТЬ";
        public bool isMandatory;
        public bool isRepeatable;
        public bool hasChoices = true;
        public int minimumDay = 1;
        public int maximumDay = 9;
        public Prototype2EventChoice left;
        public Prototype2EventChoice right;
        public Prototype2EventChoice down;
    }

    [Serializable]
    public sealed class Prototype2EventChoice
    {
        public int food;
        public int fuel;
        public int weapons;
        public int householdGoods;
        public int frontStrength;
        public int civilianReputation;
        public int defenseReputation;
        public int unionReputation;
        public string flagToAdd;
        public string flagToRemove;
        public int triggerEffectAfterDays;
        public string followUpEventId;
    }
}
