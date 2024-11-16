using HarmonyLib;

using SpawnSleepersInRange.Common;

namespace SpawnSleepersInRange.Harmony
{
    [HarmonyPatch(typeof(global::TriggerVolume))]
    [HarmonyPatch("CheckTouching")]
    public class TriggerVolumeCheckTouching
    {
        public static bool Prefix(TriggerVolume __instance, World _world, EntityPlayer _player)
        {
            if (Config.Instance.SpawningMethod == SpawningMethod.Proximity && Config.Instance.ActivateTriggersAtRange)
            {
                if (__instance.isTriggered)
                    return false;

                if(CheckUtils.IsWithinBounds(_player.position, __instance.BoxMin, __instance.BoxMax))
                {
                    __instance.Touch(_world, _player);
                }
                return false;
            }

            return true;
        }
    }
}