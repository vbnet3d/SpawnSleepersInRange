using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawnSleepersInRange.Harmony
{
    internal abstract class SpawnConsoleCommandBase : ConsoleCmdAbstract
    {
        public override bool IsExecuteOnClient => true;

        public override int DefaultPermissionLevel => 0;

        public override bool AllowedInMainMenu => true;
    }
}
