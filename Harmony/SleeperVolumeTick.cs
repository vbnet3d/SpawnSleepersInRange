using System;

using HarmonyLib;

using SpawnSleepersInRange.Common;

namespace SpawnSleepersInRange.Harmony
{
    [HarmonyPatch(typeof(global::SleeperVolume))]
    [HarmonyPatch("Tick")]
    public class SleeperVolumeTick
    {
        public static void Postfix(SleeperVolume __instance, World _world)
        {
            if (Config.Instance.SpawningMethod == SpawningMethod.Proximity)
            {
                try
                {
                    if (__instance == null || _world == null)
                    {
                        return;
                    }

                    if (!__instance.wasCleared)
                    {
                        foreach (EntityPlayer player in _world.Players.list)
                        {
                            if (Config.Instance.OnlySpawnInCurrentPOI || player.AttachedToEntity is EntityVehicle)
                            {
                                if (__instance.PrefabInstance == null || __instance.PrefabInstance != player.prefab)
                                {
                                    continue;
                                }
                            }

                            if (CheckUtils.IsWithinBounds(player.position, __instance.BoxMin, __instance.BoxMax))
                            {
                                // SleeperVolume.TouchGroup() handles *most* spawns, except for special triggers
                                __instance.TouchGroup(_world, player, false);
                                break;
                            }
                        }
                    }
                }
                catch(Exception e) {
                    Log.Warning($"SleeperVolume.Tick Postfix: {e}");
                }
            }
        }
    }
}