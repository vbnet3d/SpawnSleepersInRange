using HarmonyLib;
using SpawnSleepersInRange.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SpawnSleepersInRange.Harmony
{
    public class SpawnManager : MonoBehaviour
    {
        private static SpawnManager instance;

        private Dictionary<EntityPlayer, Queue<SleeperVolume>> toSpawn;
        private Dictionary<EntityPlayer, Queue<TriggerVolume>> toTrigger;
        private Dictionary<EntityPlayer, PrefabInstance> poiTracker;

        public bool IsRunning { get; private set; }

        public SpawnManager()
        {
            toSpawn = new Dictionary<EntityPlayer, Queue<SleeperVolume>>();
            toTrigger = new Dictionary<EntityPlayer, Queue<TriggerVolume>>();
            poiTracker = new Dictionary<EntityPlayer, PrefabInstance>();
        }

        public static SpawnManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var tempgameObject = new GameObject();
                    instance = tempgameObject.AddComponent<SpawnManager>();
                }
                return instance;
            }
        }

        public void Start()
        {
            IsRunning = true;
            StartCoroutine(SpawnHandler());
        }

        public void Stop()
        {
            IsRunning = false;
            StopAllCoroutines();
        }

        IEnumerator SpawnHandler()
        {
            while (true)
            {
                try
                {
                    foreach (var player in GameManager.Instance.World.Players.list)
                    {
                        if (player.prefab == null)
                        {
                            if (toSpawn.ContainsKey(player))
                            {
                                toSpawn[player].Clear();
                                toSpawn.Remove(player);
                            }

                            if (toTrigger.ContainsKey(player))
                            {
                                toTrigger[player].Clear();
                                toTrigger.Remove(player);
                            }

                            poiTracker.Remove(player);
                        }
                        else if (toSpawn.ContainsKey(player))
                        {
                            if (poiTracker.ContainsKey(player) && poiTracker[player] == player.prefab)
                            {
                                var sleeperVolume = toSpawn[player].Dequeue();
                                sleeperVolume.TouchGroup(GameManager.Instance.World, player, false);

                                var triggerVolume = toTrigger[player].Dequeue();
                                triggerVolume.Touch(GameManager.Instance.World, player);
                            }
                            else
                            {
                                toTrigger[player].Clear();
                                toTrigger.Remove(player);
                                toSpawn[player].Clear();
                                poiTracker.Remove(player);
                                toSpawn.Remove(player);
                            }
                        }
                        else
                        {
                            QueueSleeperVolumesForSpawn(player);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Logging.LogOnce(ex.Message);
                }

                yield return new WaitForSeconds(0.25f);
            }
        }        

        public void QueueSleeperVolumesForSpawn(EntityPlayer player)
        {
            if (player == null || player.prefab == null)
                return;

            if (toSpawn.ContainsKey(player))
            {
                toSpawn[player].Clear();
                toSpawn.Remove(player);
            }

            if (toTrigger.ContainsKey(player))
            {
                toTrigger[player].Clear();
                toTrigger.Remove(player);
            }

            toSpawn.Add(player, new Queue<SleeperVolume>());
            toTrigger.Add(player, new Queue<TriggerVolume>());

            if (poiTracker.ContainsKey(player))
                poiTracker[player] = player.prefab;
            else
                poiTracker.Add(player, player.prefab);

            foreach (var sleeperVolume in player.prefab.sleeperVolumes
                .Where(s => !s.wasCleared)
                .OrderBy(s => Vector3.Distance(s.Center, player.position)))
            {
                toSpawn[player].Enqueue(sleeperVolume);
            }

            foreach (var triggerVolume in player.prefab.triggerVolumes
                .Where(t => !t.isTriggered)
                .OrderBy(t => Vector3.Distance(t.Center, player.position)))
            {
                toTrigger[player].Enqueue(triggerVolume);
            }
        }

        internal void ResetSleeperVolumes(EntityPlayer player)
        {
            if (player == null || player.prefab == null)
                return;

            foreach (var sleeperVolume in player.prefab.sleeperVolumes)
            {
                sleeperVolume.Reset();
            }
        }
    }

    [HarmonyPatch(typeof(global::GameManager))]
    [HarmonyPatch("StartGame")]
    public class GameManager_StartGame
    {
        public static void Postfix()
        {
            if (Config.Instance.SpawningMethod == SpawningMethod.POI)
            {
                SpawnManager.Instance.Start();
            }
        }
    }

    [HarmonyPatch(typeof(global::GameManager))]
    [HarmonyPatch("SaveAndCleanupWorld")]
    public class GameManager_SaveAndCleanupWorld
    {
        public static void Postfix()
        {
            if (Config.Instance.SpawningMethod == SpawningMethod.POI)
            {
                SpawnManager.Instance.Stop();
            }
        }
    }
}

