using Epic.OnlineServices.Presence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SpawnSleepersInRange.Common
{
    public class Config
    {
        private static Config instance;
        public static Config Instance
        {
            get
            {
                if (instance == null)
                {
                    try
                    {
                        Log.Out("Loading SpawnSleepersInRange configuration...");
                        XmlSerializer serializer = new XmlSerializer(typeof(Config));
                        using (StreamReader reader = new StreamReader(".\\Mods\\SpawnSleepersInRange\\Config.xml"))
                        {
                            instance = (Config)serializer.Deserialize(reader);
                        }                            
                    }
                    catch (Exception ex)
                    {
                        Log.Out("Failed to load config for SpawnSleepersInRange. Falling back to defaults." + ex.Message);
                        instance = new Config();
                    }
                }
                return instance;
            }
            private set
            {
                instance = value;
            }
        }

        public float SpawnRadius = 30.0f;
        public bool DisableTriggers = true;
        public bool SpawnAggressive = false;
    }

}
