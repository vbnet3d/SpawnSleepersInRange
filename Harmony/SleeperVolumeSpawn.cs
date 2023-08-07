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
            catch (System.Exception ex)
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
                    if (PlayerWithinRange(__instance, player, Config.Instance.SpawnRadius))
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
