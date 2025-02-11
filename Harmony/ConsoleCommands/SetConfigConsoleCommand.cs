using SpawnSleepersInRange.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawnSleepersInRange.Harmony.ConsoleCommands
{
    internal class SetConfigConsoleCommand : SpawnConsoleCommandBase
    {
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (_params.Count > 1)
            {
                switch (_params[0].ToLower())
                {
                    case "usesplitspawnradii":
                        SetConfigBool(ref Config.Instance.UseSplitSpawnRadii, _params[1]);                        
                        break;
                    case "spawnradius":
                        SetConfigFloat(ref Config.Instance.SpawnRadius, _params[1]);
                        break;
                    case "verticalspawnradius":
                        SetConfigFloat(ref Config.Instance.VerticalSpawnRadius, _params[1]);
                        break;
                    case "disabletriggers":
                        SetConfigBool(ref Config.Instance.DisableTriggers, _params[1]);
                        break;
                    case "spawnaggressive":
                        SetConfigBool(ref Config.Instance.SpawnAggressive, _params[1]);
                        break;
                    case "allowclearquesttriggers":
                        SetConfigBool(ref Config.Instance.AllowClearQuestTriggers, _params[1]);
                        break;
                    case "onlyspawnincurrentpoi":
                        SetConfigBool(ref Config.Instance.OnlySpawnInCurrentPOI, _params[1]);
                        break;
                    case "activatetriggersatrange":
                        SetConfigBool(ref Config.Instance.ActivateTriggersAtRange, _params[1]);
                        break;
                    case "spawningmethod":
                        if (_params[1].ToLower() == "poi")
                            Config.Instance.SpawningMethod = SpawningMethod.POI;
                        else
                            Config.Instance.SpawningMethod = SpawningMethod.Proximity;
                        break;
                    default:
                        PrintUse();
                        break;
                }               
            }
            else
            {
                PrintUse();
            }
        }

        private void SetConfigFloat(ref float property, string v)
        {
            if (float.TryParse(v, out float value))
                property = value;
        }

        private void SetConfigBool(ref bool property, string v)
        {
            if (bool.TryParse(v, out bool value))
                property = value;            
        }

        private void PrintUse()
        {
            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Use: spawn.config [property] [value]");
            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Current Configuration:");
            foreach (string config in Config.Instance.ToString())
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(config);
            }
        }

        public override string[] getCommands()
        {
            return new string[] { "spawn.config" };
        }

        public override string getDescription()
        {
            return "Set the configuration values for SpawnSleepersInRange. Use spawn.config help for details.";
        }
    }


}
