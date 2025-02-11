using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawnSleepersInRange.Harmony.ConsoleCommands
{
    internal class ResetSpawnsConsoleCommand : SpawnConsoleCommandBase
    {
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            EntityPlayer primaryPlayer = (EntityPlayer)GameManager.Instance.World.GetPrimaryPlayer();
            SpawnManager.Instance.ResetSleeperVolumes(primaryPlayer);
        }

        public override string[] getCommands()
        {
            return new string[] { "spawn.reset" };
        }

        public override string getDescription()
        {
            return "Reset all sleeper volumes in the current POI";
        }
    }
}
