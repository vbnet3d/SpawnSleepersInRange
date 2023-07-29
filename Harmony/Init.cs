using System.Reflection;

namespace SpawnSleepersInRange.Harmony
{
    internal class Init : IModApi
    {
        public void InitMod(Mod _modInstance)
        {
            Log.Out(" Loading Patch: " + GetType());

            Log.Out(" Config.SpawnRadius: " + Common.Config.SpawnRadius);
            Log.Out(" Config.DisableTriggers: " + Common.Config.DisableTriggers.ToString());

            var harmony = new HarmonyLib.Harmony(GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
