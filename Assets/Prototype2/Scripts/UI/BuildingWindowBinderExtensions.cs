namespace Baricovats.Prototype2
{
    public static class BuildingWindowBinderExtensions
    {
        public static bool BuildingWindowBinderExists()
        {
            return UnityEngine.Object.FindObjectsByType<BuildingWindowBinder>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None).Length > 0;
        }
    }
}
