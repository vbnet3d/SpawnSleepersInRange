using System.Reflection;

namespace SpawnSleepersInRange.Harmony
{
    internal class Init : IModApi
    {
        public void InitMod(Mod _modInstance)
        {
            Log.Out(" Loading Patch: " + GetType());

            foreach (string option in Common.Config.Instance.ToString())
            {
                Log.Out(option);
            }          

            var harmony = new HarmonyLib.Harmony(GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }        
    }
}