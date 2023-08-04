using System.Reflection;

namespace SpawnSleepersInRange.Harmony
{
    internal class Init : IModApi
    {
        public void InitMod(Mod _modInstance)
        {
            Log.Out(" Loading Patch: " + GetType());

            Log.Out(" Config.SpawnRadius: " + Common.Config.Instance.SpawnRadius);
            Log.Out(" Config.DisableTriggers: " + Common.Config.Instance.DisableTriggers.ToString());
            Log.Out(" Config.SpawnAggressive: " + Common.Config.Instance.SpawnAggressive.ToString());

            var harmony = new HarmonyLib.Harmony(GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
