using HarmonyLib;
using SpawnSleepersInRange.Common;
using System.Reflection;

namespace SpawnSleepersInRange.Harmony
{
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
}
