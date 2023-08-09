using HarmonyLib;
using SpawnSleepersInRange.Common;
using System;
using System.Collections.Generic;
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

    [HarmonyPatch(typeof(global::SleeperVolume))]
    [HarmonyPatch("Tick")]
    public class SleeperVolumeTick
    {
        private static MethodInfo touchGroup;
        private static FieldInfo hasPassives;
        private static HashSet<string> logOnceMessages = new HashSet<string>();

        private static void LogOnce(string message)
        {
            if (logOnceMessages.Contains(message))
            {
                return;
            }
            else
            {
                Log.Out(message);
                logOnceMessages.Add(message);
            }
        }

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
                        LogOnce("Player is in vehicle");

                        if (!__instance.PrefabInstance.IsWithinInfoArea(player.position))
                        {
                            // skip this player. It only makes sense to spawn sleepers if the player in a vehicle is actually within the POI's boundaries
                            // if someone is cruising through town, we don't want dozens of sleepers spawning in and out every second
                            continue;
                        }
                        else
                        {
                            LogOnce("Player is inside POI: " + __instance.PrefabInstance.name);
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

    [HarmonyPatch(typeof(global::SleeperVolume))]
    [HarmonyPatch("OnTriggered")]
    public class SleeperVolumeOnTriggered
    {
        private static MethodInfo updatePlayerTouched;

        public static bool Prefix(SleeperVolume __instance, EntityPlayer _player, World _world)
        {
            if (Config.Instance.DisableTriggers)
            {
                if (Config.Instance.AllowClearQuestTriggers)
                {
                    if (updatePlayerTouched == null)
                    {
                        updatePlayerTouched = typeof(SleeperVolume).GetMethod("UpdatePlayerTouched", BindingFlags.Instance | BindingFlags.NonPublic);
                    }

                    updatePlayerTouched.Invoke(__instance, new object[] { _world, _player });
                }

                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(global::NavObject))]
    [HarmonyPatch("IsValidEntity")]
    public class NavObjectIsValidEntity
    {
        public static bool Prefix(ref bool __result, EntityPlayerLocal player, Entity entity, NavObjectClass navObjectClass)
        {
            __result = IsValidEntity(player, entity, navObjectClass);

            return false;
        }

        private static bool IsValidEntity(EntityPlayerLocal player, Entity entity, NavObjectClass navObjectClass)
        {
            if ((UnityEngine.Object)entity == (UnityEngine.Object)null || (UnityEngine.Object)player == (UnityEngine.Object)null)
                return true;
            if (entity is EntityAlive)
            {
                EntityAlive entityAlive = entity as EntityAlive;
                if (navObjectClass.RequirementType == NavObjectClass.RequirementTypes.None)
                    return entityAlive.IsAlive();
                if (!entityAlive.IsAlive())
                    return false;
                switch (navObjectClass.RequirementType)
                {
                    case NavObjectClass.RequirementTypes.CVar:
                        return (double)entityAlive.GetCVar(navObjectClass.RequirementName) > 0.0;
                    case NavObjectClass.RequirementTypes.QuestBounds:
                        if (player.QuestJournal.ActiveQuest != null && entityAlive.IsSleeper)
                        {
                            Vector3 position = entity.position;
                            position.y = position.z;
                            if (player.ZombieCompassBounds.Contains(position))
                                return true;
                        }
                        return false;
                    case NavObjectClass.RequirementTypes.Tracking:
                        return (double)EffectManager.GetValue(PassiveEffects.Tracking, _entity: (EntityAlive)player, tags: entity.EntityTags) > 0.0;
                    case NavObjectClass.RequirementTypes.InParty:
                        return player.Party != null && player.Party.MemberList.Contains(entity as EntityPlayer) && (UnityEngine.Object)entity != (UnityEngine.Object)player && !(entity as EntityPlayer).IsSpectator;
                    case NavObjectClass.RequirementTypes.IsAlly:
                        return (UnityEngine.Object)(entity as EntityPlayer) != (UnityEngine.Object)null && (entity as EntityPlayer).IsFriendOfLocalPlayer && (UnityEngine.Object)entity != (UnityEngine.Object)player && !(entity as EntityPlayer).IsSpectator;
                    case NavObjectClass.RequirementTypes.IsPlayer:
                        return (UnityEngine.Object)entity == (UnityEngine.Object)player;
                    case NavObjectClass.RequirementTypes.IsVehicleOwner:
                        if ((UnityEngine.Object)(entity as EntityVehicle) != (UnityEngine.Object)null && (entity as EntityVehicle).HasOwnedEntity(player.entityId))
                            return true;
                        return (UnityEngine.Object)(entity as EntityTurret) != (UnityEngine.Object)null && (entity as EntityTurret).belongsPlayerId == player.entityId;
                    case NavObjectClass.RequirementTypes.NoActiveQuests:
                        return (UnityEngine.Object)(entity as EntityNPC) == (UnityEngine.Object)null || player.QuestJournal.FindReadyForTurnInQuestByGiver(entity.entityId) == null;
                    case NavObjectClass.RequirementTypes.IsTwitchSpawnedSelf:
                        return entity.spawnById == player.entityId && !string.IsNullOrEmpty(entity.spawnByName);
                    case NavObjectClass.RequirementTypes.IsTwitchSpawnedOther:
                        return entity.spawnById > 0 && entity.spawnById != player.entityId && !string.IsNullOrEmpty(entity.spawnByName);
                }
            }
            else
            {
                switch (navObjectClass.RequirementType)
                {
                    case NavObjectClass.RequirementTypes.IsTwitchSpawnedSelf:
                        return entity.spawnById == player.entityId;
                    case NavObjectClass.RequirementTypes.IsTwitchSpawnedOther:
                        return entity.spawnById > 0 && entity.spawnById != player.entityId;
                }
            }
            return true;
        }
    }
}
