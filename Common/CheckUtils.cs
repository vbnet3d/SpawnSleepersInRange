namespace SpawnSleepersInRange.Common
{
    using UnityEngine;

    public static class CheckUtils
    {
        // Use the same checking method for both sleeper and trigger volumes for consistency.
        public static bool IsWithinBounds(Vector3 pos, Vector3i min, Vector3i max)
        {
            pos.y += 0.8f;
            int offsetHorizontal = (int)Config.Instance.SpawnRadius;
            int offsetVertical = (int)Config.Instance.VerticalSpawnRadius;
            Vector3i offset = new Vector3i(offsetHorizontal, offsetVertical, offsetHorizontal);
            min -= offset;
            max += offset;
            return pos.x >= min.x && pos.x <= max.x && pos.y >= min.y && pos.y <= max.y && pos.z >= min.z && pos.z <= max.z;
        }
    }
}