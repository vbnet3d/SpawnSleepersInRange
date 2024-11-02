# SpawnSleepersInRange

Experimental mod for 7 Day to Die (V 1.0) that forces sleeper volumes to spawn in sleepers within a certain range, even if the trigger is not yet activated. Intended to fix issues with pop-in spawning and clear quests failing to spawn in all zombies.

WARNING: This is an entirely experimental mod. Use at your own risk. May affect frame rates if it spawns in too many zombies at one time.

## Configuration

Mod options can be changed by editing Config.xml in the mod folder.

| Config Field | Valid Values | Effects |
|--|--|--|
|**UseSplitSpawnRadii**| true or false | If true, splits distance checks into horizontal and vertical. If false, uses the overall 3D distance for spawn checks. |
|**SpawnRadius**| Integer or floating-point number (e.g. 30, 15.0) | The spawn distance cutoff in Meters. If UseSplitSpawnRadii is true, this is used for the horizontal distance check.
|**VerticalSpawnRadius**| Integer or floating-point number (e.g. 10, 10.0) | When UseSplitSpawnRadii is true, defines the vertical distance cutoff in Meters.
|**DisableTriggers**|true or false|If true, surprise triggers will be disabled. Clear quest triggers can be enabled separately with AllowClearQuestTriggers. If false, all triggers will occur as originally configured.
|**SpawnAggressive**|true or false| If true, zombies spawn aggro'ed, and may hunt the player down. This makes stealth more challenging. If false, zombies spawn passive/sleeping as normal.
|**AllowClearQuestTriggers**|true or false|If true, POI triggers related specifically to clear quests will still be run. Many clear quests cannot be completed with this set to false.
|**OnlySpawnInCurrentPOI**|true or false|If true, only spawns Zombies if the player is within the bounds of a POI. Can save FPS in large cities with buildings close together.
|**ActivateTriggersAtRange**|true or false|If true, adds the spawn radius and vertical spawn radius to trigger proximity checks so that they trigger further out. This should reduce the jump scare effect of triggered spawns.

## Installation

Download source to your 7 Days to Die mod folder (usually C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods).

This is a Harmony (C#) mod. EAC must be disabled.

Inspired by [SphereII's modding work](https://github.com/SphereII/SphereII.Mods).
