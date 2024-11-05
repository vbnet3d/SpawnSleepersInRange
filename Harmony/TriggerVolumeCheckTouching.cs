using HarmonyLib;
using SpawnSleepersInRange.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

                Vector3 position = _player.position;
                position.y += 0.8f;

                Vector3i max = __instance.BoxMax + new Vector3i(Config.Instance.SpawnRadius / 2, Config.Instance.VerticalSpawnRadius / 2, Config.Instance.SpawnRadius / 2);
                Vector3i min = __instance.BoxMin - new Vector3i(Config.Instance.SpawnRadius / 2, Config.Instance.VerticalSpawnRadius / 2, Config.Instance.SpawnRadius / 2);

                if (position.x < (double)min.x || position.x >= (double)max.x || position.y < (double)min.y || position.y >= (double)max.y || position.z < (double)min.z || position.z >= (double)max.z)
                    return false;

                __instance.Touch(_world, _player);

                return false;
            }

            return true;
        }
    }
}
