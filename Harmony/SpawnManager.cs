using HarmonyLib;
using SpawnSleepersInRange.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SpawnSleepersInRange.Harmony
{
    public class POISpawnTracking
    {
        public DateTime LastPlayerInPOI { get; set; }
        public EntityPlayer MostRecentPlayer { get; set; }
        public PrefabInstance POIPrefab { get; set; }
        public List<SleeperVolume> SleeperVolumes { get; set; }
        public Queue<TriggerVolume> TriggerVolumes { get; set; }
    }

    public class SpawnManager : MonoBehaviour
    {
        private static SpawnManager instance;
        private List<POISpawnTracking> tracker = new List<POISpawnTracking>();

        public bool IsRunning { get; private set; }

        public SpawnManager()
        {
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
            StartCoroutine(SpawnCheckLoop());
        }

        public void Stop()
        {
            IsRunning = false;
            StopAllCoroutines();
        }

        IEnumerator SpawnCheckLoop()
        {
            while (IsRunning)
            {
                try
                {
                    UpdatePOISpawnTracking();
                    RemoveStalePOIs();
                    ProcessPOISpawnsAndTriggers();
                }
                catch (Exception ex)
                {
                    Logging.LogOnce(ex.Message);
                }
                yield return new WaitForSeconds(0.5f);
            }
        }

        private void ProcessPOISpawnsAndTriggers()
        {
            foreach (var poi in tracker)
            {
                var unclearedSleeperVolumes = poi.SleeperVolumes
                    .Where(s => !s.wasCleared)
                    .ToList();

                foreach (var player in GameManager.Instance.World.Players.list)
                {
                    // if player is not null and within spawn radius, spawn sleeper volumes
                    if (player != null && poi.POIPrefab.Equals(player.prefab))
                    {
                        var toSpawn = unclearedSleeperVolumes.Where(s => IsWithinBounds(player.position, s.BoxMin, s.BoxMax));

                        foreach (var sleeperVolume in toSpawn)
                        {
                            sleeperVolume.TouchGroup(GameManager.Instance.World, player, Config.Instance.SpawnAggressive);
                        }
                    }
                }

                // check for trigger volumes
                if (poi.TriggerVolumes.Count > 0)
                {
                    var trigger = poi.TriggerVolumes.Dequeue();
                    if (!trigger.isTriggered)
                    {
                        trigger.Touch(GameManager.Instance.World, poi.MostRecentPlayer);
                    }
                }
            }
        }

        private void RemoveStalePOIs()
        {
            // check for POIs that have not been visited by a player in the last 30 seconds
            foreach (var poi in tracker)
            {
                if (poi.LastPlayerInPOI < DateTime.Now.AddSeconds(-30))
                {
                    foreach (var sleeperVolume in poi.SleeperVolumes)
                    {
                        sleeperVolume.Reset();
                        sleeperVolume.Despawn(GameManager.Instance.World);
                    }
                }
            }
            tracker.RemoveAll(p => p.LastPlayerInPOI < DateTime.Now.AddSeconds(-30));
        }

        private void UpdatePOISpawnTracking()
        {
            foreach (var player in GameManager.Instance.World.Players.list)
            {
                // is player inside a POI?
                if (player.prefab != null)
                {
                    var poi = tracker.FirstOrDefault(p => p.POIPrefab.Equals(player.prefab));
                    if (poi == null)
                    {
                        poi = new POISpawnTracking
                        {
                            POIPrefab = player.prefab,
                            SleeperVolumes = new List<SleeperVolume>(),
                            TriggerVolumes = new Queue<TriggerVolume>(),
                            LastPlayerInPOI = DateTime.Now,
                            MostRecentPlayer = player
                        };

                        poi.SleeperVolumes = player.prefab.sleeperVolumes
                            .Where(s => !s.wasCleared && !IsInsideLandClaimBlock(s.Center))
                            .OrderBy(s => Vector3.Distance(s.Center, player.position))
                            .ToList();

                        var triggers = player.prefab.triggerVolumes
                            .Where(t => !t.isTriggered)
                            .OrderBy(t => Vector3.Distance(t.Center, player.position));

                        foreach (var trigger in triggers)
                        {
                            poi.TriggerVolumes.Enqueue(trigger);
                        }

                        tracker.Add(poi);
                    }
                    else
                    {
                        poi.LastPlayerInPOI = DateTime.Now;
                        poi.MostRecentPlayer = player;
                    }
                }
            }
        }

        private bool IsInsideLandClaimBlock(Vector3 center)
        {
            return false;
        }

        public static bool IsWithinBounds(Vector3 pos, Vector3i min, Vector3i max)
        {
            pos.y += 0.8f;
            int offsetHorizontal = (int)Config.Instance.SpawnRadius;
            int offsetVertical = (int)Config.Instance.VerticalSpawnRadius;
            Vector3i offset = new Vector3i(offsetHorizontal, offsetVertical, offsetHorizontal);
            min -= offset;
            max += offset;
            return pos.x >= min.x && pos.x <= max.x && pos.y >= min.y && pos.y <= max.y && pos.z >= min.z && pos.z <= max.z;
        }
    }

    [HarmonyPatch(typeof(global::GameManager))]
    [HarmonyPatch("StartGame")]
    public class GameManager_StartGame
    {
        public static void Postfix()
        {
            SpawnManager.Instance.Start();
        }
    }

    [HarmonyPatch(typeof(global::GameManager))]
    [HarmonyPatch("SaveAndCleanupWorld")]
    public class GameManager_SaveAndCleanupWorld
    {
        public static void Postfix()
        {
            SpawnManager.Instance.Stop();
        }
    }
}

