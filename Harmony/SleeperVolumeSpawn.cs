using HarmonyLib;
using SpawnSleepersInRange.Common;
using System.Reflection;
using UnityEngine;

namespace SpawnSleepersInRange.Harmony
{
    [HarmonyPatch(typeof(global::SleeperVolume))]
    [HarmonyPatch("Spawn")]
    public class SleeperVolumeSpawn
    {
        public static void Postfix(ref EntityAlive __result, int ___flags)
        {
            __result.SetSleeperActive();
            __result.ResumeSleeperPose();
            __result.IsSleeperPassive = true;
        }
    }

    [HarmonyPatch(typeof(global::SleeperVolume))]
    [HarmonyPatch("Tick")]
    public class SleeperVolumeTick
    {
        private static MethodInfo touchGroup;

        public static void Postfix(SleeperVolume __instance, World _world)
        {
            if (!__instance.wasCleared)
            {
                if (touchGroup == null)
                {
                    touchGroup = typeof(SleeperVolume).GetMethod("TouchGroup", BindingFlags.Instance | BindingFlags.NonPublic);
                }

                foreach (EntityPlayer player in _world.Players.list)
                {
                    if (PlayerWithinRange(__instance, player, Config.SpawnRadius))
                    {
                        touchGroup.Invoke(__instance, new object[] { _world, player, false });
                    }
                }
            }
        }

        private static bool PlayerWithinRange(SleeperVolume volume, EntityPlayer player, float range)
        {
            return Vector3.Distance(volume.Center, player.position) <= range;
        }
    }

    [HarmonyPatch(typeof(global::SleeperVolume))]
    [HarmonyPatch("OnTriggered")]
    public class SleeperVolumeOnTriggered
    {
        public static bool Prefix()
        {
            return !Config.DisableTriggers;
        }
    }
}
