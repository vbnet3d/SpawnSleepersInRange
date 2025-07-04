using HarmonyLib;
using SpawnSleepersInRange.Common;
using System;
using System.Collections.Generic;

namespace SpawnSleepersInRange.Harmony
{
    [HarmonyPatch(typeof(global::SleeperVolume))]
    [HarmonyPatch("Spawn")]
    public class SleeperVolumeSpawn
    {
        public static void Postfix(SleeperVolume __instance, ref EntityAlive __result, int ___flags)
        {
            if (__result == null)
            {
                return;
            }

            try
            {
                __result.SetSleeperActive();
                __result.ResumeSleeperPose();
                __instance.respawnTime = Math.Max(__instance.respawnTime, GameManager.Instance.World.worldTime + 1000);
                __result.IsSleeperPassive = !Config.Instance.SpawnAggressive;
            }
            catch (Exception ex)
            {
                Log.Out("SpawnSleepersInRange::SleeperVolumeSpawn failed: " + ex.Message);
            }
        }
    }

    [HarmonyPatch(typeof(global::SleeperVolume))]
    [HarmonyPatch("Reset")]
    public class SleeperVolumeReset
    {
        public static bool Prefix(SleeperVolume __instance)
        {
            if (!IsInLandClaim(__instance))
                Reset(__instance);

            return false;
        }

        private static void Reset(SleeperVolume instance)
        {
            instance.playerTouchedToUpdate = (EntityPlayer)null;
            instance.playerTouchedTrigger = (EntityPlayer)null;
            instance.respawnTime = ulong.MaxValue;
            instance.isSpawning = false;
            instance.isSpawned = false;
            instance.wasCleared = false;
            instance.numSpawned = 0;
            instance.respawnMap.Clear();
            instance.respawnList = (List<int>)null;
            if (instance.minScript == null)
                return;
            instance.minScript.Reset();
        }

        private static bool IsInLandClaim(SleeperVolume instance)
        {
            //var pos = new Vector3i(instance.Center.x, instance.Center.y, instance.Center.z);
            //var chunk = GameManager.Instance.World.GetChunkFromWorldPos(pos);
            //var lcbs = GameManager.Instance.World.IsLandProtectedBlock(chunk, pos, )

            return false;
        }
    }
}
