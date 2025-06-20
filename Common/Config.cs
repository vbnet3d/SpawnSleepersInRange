using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
                        string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        Log.Out("Loading SpawnSleepersInRange configuration...");
                        XmlSerializer serializer = new XmlSerializer(typeof(Config));
                        using (StreamReader reader = new StreamReader(assemblyFolder + "\\Config.xml"))
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

        private static FieldInfo[] fields;
        public new IEnumerable<string> ToString()
        {
            if (fields == null)
            {
                fields = typeof(Config).GetFields(BindingFlags.Instance | BindingFlags.Public);
            }

            foreach (var field in fields)
            {
                yield return "Config." + field.Name + ": " + field.GetValue(this).ToString();
            }
        }

        public float SpawnRadius = 30.0f; // either total spawn radius (3D) or horizontal spawn radius       
        public float VerticalSpawnRadius = 10.0f;  
        public bool SpawnAggressive = false;
    }

    public enum SpawningMethod
    {
        Proximity,POI
    }

}
