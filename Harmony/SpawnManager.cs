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
        private List<POISpawnTracking> poiSpawnTracking = new List<POISpawnTracking>();

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

        /*
         Update algorithm:
            - For each player check if they are in the bounds of a POI.
            - If they are, check if the POI is already tracked for spawns and triggers. If POI is already cleared, do nothing.
            - If not, cache collection of sleeper volumes and queue for trigger volumes for that POI sorted by distance from the player. 
                Exclude all that are within the boundaries of a Land Claim Block
            - For sleeper volumes, do distance check in a coroutine against the player position and the volume center. Spawn if within range.
            - For trigger volumes, run through a coroutine and trigger them sequentially with a delay.
            - If no player has been in a POI for > 30 seconds, clear the queues and collections for that POI.
         */

        IEnumerator SpawnCheckLoop()
        {
            while (IsRunning)
            {
                try
                {
                    QueueNewPOIs();

                    // check for POIs that have not been visited by a player in the last 30 seconds
                    foreach (var poi in poiSpawnTracking)
                    {
                        if (poi.LastPlayerInPOI < DateTime.Now.AddSeconds(-30))
                        {
                            foreach (var sleeperVolume in poi.SleeperVolumes)
                            {
                                sleeperVolume.Reset();
                            }
                        }
                    }
                    poiSpawnTracking.RemoveAll(p => p.LastPlayerInPOI < DateTime.Now.AddSeconds(-30));

                    foreach (var poi in poiSpawnTracking)
                    {
                        // check if there are any sleeper volumes left to spawn
                        if (poi.SleeperVolumes.Count > 0)
                        {
                            foreach (var player in GameManager.Instance.World.Players.list)
                            {
                                // if player is not null and within spawn radius, spawn sleeper volumes
                                if (player != null && poi.POIPrefab.Equals(player.prefab))
                                {
                                    var toSpawn = poi.SleeperVolumes
                                        .Where(s => !s.wasCleared && CheckDistance(s.Center, player));

                                    foreach (var sleeperVolume in toSpawn)
                                    {
                                       sleeperVolume.TouchGroup(GameManager.Instance.World, player, Config.Instance.SpawnAggressive);
                                    }
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
                catch (Exception ex)
                {
                    Logging.LogOnce(ex.Message);
                }
                yield return new WaitForSeconds(0.5f);
            }
        }

        private void QueueNewPOIs()
        {
            foreach (var player in GameManager.Instance.World.Players.list)
            {
                // is player inside a POI?
                if (player.prefab != null)
                {
                    var poi = poiSpawnTracking.FirstOrDefault(p => p.POIPrefab.Equals(player.prefab));
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

                        poiSpawnTracking.Add(poi);
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

        private static bool CheckDistance(Vector3 point, EntityPlayer player)
        {
            if (Config.Instance.UseSplitSpawnRadii)
            {
                if (Math.Abs(point.y - player.position.y) <= Config.Instance.VerticalSpawnRadius)
                {
                    if (Vector2.Distance(new Vector2(point.x, point.z), new Vector2(player.position.x, player.position.z)) <= Config.Instance.SpawnRadius)
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

