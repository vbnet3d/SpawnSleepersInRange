using SpawnSleepersInRange.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawnSleepersInRange.Harmony.ConsoleCommands
{
    internal class TriggerAllSpawnsConsoleCommand : SpawnConsoleCommandBase
    {
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            EntityPlayer primaryPlayer = (EntityPlayer)GameManager.Instance.World.GetPrimaryPlayer();
            SpawnManager.Instance.QueueSleeperVolumesForSpawn(primaryPlayer);
        }

        public override string[] getCommands()
        {
            return new string[] { "spawn.trigger" };
        }

        public override string getDescription()
        {
            return "Trigger all active sleeper volumes to spawn in the current POI";
        }
    }
}
