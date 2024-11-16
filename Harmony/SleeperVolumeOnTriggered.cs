using HarmonyLib;

using SpawnSleepersInRange.Common;

namespace SpawnSleepersInRange.Harmony
{
    [HarmonyPatch(typeof(global::SleeperVolume))]
    [HarmonyPatch("OnTriggered")]
    public class SleeperVolumeOnTriggered
    { 
        public static bool Prefix(SleeperVolume __instance, EntityPlayer _player, World _world)
        {
            if (Config.Instance.DisableTriggers)
            {
                if (Config.Instance.AllowClearQuestTriggers)
                {
                    __instance.UpdatePlayerTouched(_world, _player);                    
                }

                return false;
            }

            return true;
        }
    }
}