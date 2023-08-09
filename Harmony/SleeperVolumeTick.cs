using HarmonyLib;
using SpawnSleepersInRange.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SpawnSleepersInRange.Harmony
{
    [HarmonyPatch(typeof(global::SleeperVolume))]
    [HarmonyPatch("Tick")]
    public class SleeperVolumeTick
    {
        private static MethodInfo touchGroup;
        private static FieldInfo hasPassives;

        public static void Postfix(SleeperVolume __instance, World _world)
        {
            if (__instance == null || _world == null)
            {
                return;
            }

            if (hasPassives == null)
            {
                hasPassives = typeof(SleeperVolume).GetField("hasPassives", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            if (!__instance.wasCleared && !(bool)hasPassives.GetValue(__instance))
            {
                if (touchGroup == null)
                {
                    touchGroup = typeof(SleeperVolume).GetMethod("TouchGroup", BindingFlags.Instance | BindingFlags.NonPublic);
                }

                foreach (EntityPlayer player in _world.Players.list)
                {
                    if (Config.Instance.OnlySpawnInCurrentPOI)
                    {
                        if (!__instance.PrefabInstance.IsWithinInfoArea(player.position))
                        {
                            continue;
                        }
                    }

                    if (player.AttachedToEntity is EntityVehicle)
                    {
                        Logging.LogOnce("Player is in vehicle");

                        if (!__instance.PrefabInstance.IsWithinInfoArea(player.position))
                        {
                            // skip this player. It only makes sense to spawn sleepers if the player in a vehicle is actually within the POI's boundaries
                            // if someone is cruising through town, we don't want dozens of sleepers spawning in and out every second
                            continue;
                        }
                        else
                        {
                            Logging.LogOnce("Player is inside POI: " + __instance.PrefabInstance.name);
                        }
                    }

                    if (PlayerWithinRange(__instance, player))
                    {
                        // SleeperVolume.TouchGroup() handles *most* spawns, except for special triggers
                        touchGroup.Invoke(__instance, new object[] { _world, player, false });
                        break;
                    }
                }
            }
        }

        private static bool PlayerWithinRange(SleeperVolume volume, EntityPlayer player)
        {
            if (Config.Instance.SpawnRadius <= 15.0f)
            {
                List<Vector3> points = new List<Vector3> { volume.Center };
                points.AddRange(CalculateBoxCorners(volume.BoxMax, volume.BoxMin));

                foreach (Vector3 point in points)
                {
                    if (CheckDistance(point, player))
                    {
                        return true;
                    }
                }

                return false;
            }
            else
            {
                return CheckDistance(volume.Center, player);
            }
        }

        private static bool CheckDistance(Vector3 point, EntityPlayer player)
        {
            if (Config.Instance.UseSplitSpawnRadii)
            {
                if (Math.Abs(point.y - player.position.y) <= Config.Instance.VerticalSpawnRadius)
                {
                    if (Vector2.Distance(point, player.position) <= Config.Instance.SpawnRadius)
                    {
                        return true;
                    }
                }

                return false;
            }
            else
            {
                return Vector3.Distance(point, player.position) <= Config.Instance.SpawnRadius;
            }
        }

        private static Vector3[] CalculateBoxCorners(Vector3 boxMax, Vector3 boxMin)
        {
            var corners = new Vector3[8];

            corners[0] = new Vector3(boxMax.x, boxMax.y, boxMax.z);
            corners[1] = new Vector3(boxMax.x, boxMax.y, boxMin.z);
            corners[2] = new Vector3(boxMax.x, boxMin.y, boxMax.z);
            corners[3] = new Vector3(boxMax.x, boxMin.y, boxMin.z);
            corners[4] = new Vector3(boxMin.x, boxMax.y, boxMax.z);
            corners[5] = new Vector3(boxMin.x, boxMax.y, boxMin.z);
            corners[6] = new Vector3(boxMin.x, boxMin.y, boxMax.z);
            corners[7] = new Vector3(boxMin.x, boxMin.y, boxMin.z);

            return corners;
        }
    }
}
