using System.Collections.Generic;

namespace SpawnSleepersInRange.Common
{
    public static class Logging
    {
        public static HashSet<string> logOnceMessages = new HashSet<string>();

        public static void LogOnce(string message)
        {
            if (logOnceMessages.Contains(message))
            {
                return;
            }
            else
            {
                Log.Out(message);
                logOnceMessages.Add(message);
            }
        }
    }
}