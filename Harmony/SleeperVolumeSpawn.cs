using HarmonyLib;
using SpawnSleepersInRange.Common;
using System;

namespace SpawnSleepersInRange.Harmony
{
    [HarmonyPatch(typeof(global::SleeperVolume))]
    [HarmonyPatch("Spawn")]
    public class SleeperVolumeSpawn
    {
        public static void Postfix(ref EntityAlive __result, int ___flags)
        {
            if (__result == null)
            {
                return;
            }

            try
            {
                __result.SetSleeperActive();
                __result.ResumeSleeperPose();

                __result.IsSleeperPassive = !Config.Instance.SpawnAggressive;
            }
            catch (Exception ex)
            {
                Log.Out("SpawnSleepersInRange::SleeperVolumeSpawn failed: " + ex.Message);
            }
        }
    }
}
