using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Globalization;

namespace GameUtils
{
    /// <summary>
    /// Clase generica para manejo de configuracion. Un archivo de configuracion leido por esta clase se compone 
    /// de secciones, dentro de las cuales pueden haber parametros, arreglos o dictionaries
    /// </summary>
    public class Configuration
    {
       
        private CultureInfo englishCulture = new CultureInfo("en-US");
        public CultureInfo Culture { get { return englishCulture; } }
        private Dictionary<string, ConfigurationSection> configSectionTable;

        private static Configuration instance;
        public static Configuration Instance
        {
            get
            {
                if (instance == null)
                    instance = new Configuration();
                return instance;
            }
        }


        private Configuration()
        {
            configSectionTable = new Dictionary<string, ConfigurationSection>();
        }

        public void Load(string configFile)
        {
            XDocument xmlDoc = XDocument.Load(configFile);
            XElement xmlConfig = xmlDoc.Elements("config").First();
            foreach (XElement sectionNode in xmlConfig.Descendants("section"))
            {
                ConfigurationSection configSection = new ConfigurationSection();
                configSection.Load(sectionNode);
                configSectionTable.Add(sectionNode.Attribute("name").Value, configSection);
            }
        }

        public string GetStringParam(string section, string key)
        {
            if (!configSectionTable.ContainsKey(section))
                throw new Exception("Invalid configuration parameter key.");
            return configSectionTable[section].GetParam(key);
        }

        public int GetIntParam(string section, string key)
        {
            if (!configSectionTable.ContainsKey(section))
                throw new Exception("Invalid configuration parameter key.");
            return int.Parse(configSectionTable[section].GetParam(key));
        }

        public double GetDoubleParam(string section, string key)
        {
            if (!configSectionTable.ContainsKey(section))
                throw new Exception("Invalid configuration parameter key.");
            return double.Parse(configSectionTable[section].GetParam(key), englishCulture);
        }

        public float GetFloatParam(string section, string key)
        {
            if (!configSectionTable.ContainsKey(section))
                throw new Exception("Invalid configuration parameter key.");
            return float.Parse(configSectionTable[section].GetParam(key), englishCulture);
        }

        public float[] GetFloatParamArray(string section, string key)
        {
            if (!configSectionTable.ContainsKey(section))
                throw new Exception("Invalid configuration parameter key.");
            string[] stringArray = configSectionTable[section].GetParam(key).Split(',');
            float[] floatArray = new float[stringArray.Length];
            for(int i=0; i<stringArray.Length; i++)
                floatArray[i] = float.Parse(stringArray[i], englishCulture);
            return floatArray;
        }

        public bool GetBoolParam(string section, string key)
        {
            if (!configSectionTable.ContainsKey(section))
                throw new Exception("Invalid configuration parameter key.");
            return configSectionTable[section].GetParam(key).Equals("true");
        }

        public List<string> GetStringArray(string section, string key)
        {
            if (!configSectionTable.ContainsKey(section))
                throw new Exception("Invalid configuration parameter key.");
            return configSectionTable[section].GetArray(key);
        }

        public Dictionary<string,string> GetStringMap(string section, string key)
        {
            if (!configSectionTable.ContainsKey(section))
                throw new Exception("Invalid configuration parameter key.");
            return configSectionTable[section].GetMap(key);
        }

        class ConfigurationSection
        {
            private Dictionary<string, string> configParams;
            private Dictionary<string, List<string>> configArrays;
            private Dictionary<string, Dictionary<string, string>> configMaps;

            public ConfigurationSection()
            {
                configParams = new Dictionary<string, string>();
                configArrays = new Dictionary<string, List<string>>();
                configMaps = new Dictionary<string, Dictionary<string, string>>();
            }

            public void Load(XElement xmlSection)
            {
                foreach (XElement paramNode in xmlSection.Elements("param"))
                    configParams.Add(paramNode.Attribute("name").Value, paramNode.Attribute("value").Value);
                foreach (XElement arrayNode in xmlSection.Elements("array"))
                {
                    List<string> arrayList = new List<string>();
                    configArrays.Add(arrayNode.Attribute("name").Value, arrayList);
                    foreach (XElement itemNode in arrayNode.Elements("item"))
                    {
                        arrayList.Add(itemNode.Attribute("value").Value);
                    }
                }
                foreach (XElement mapNode in xmlSection.Elements("map"))
                {
                    Dictionary<string, string> map = new Dictionary<string, string>();
                    configMaps.Add(mapNode.Attribute("name").Value, map);
                    foreach (XElement itemNode in mapNode.Elements("item"))
                    {
                        map.Add(itemNode.Attribute("key").Value, itemNode.Attribute("value").Value);
                    }
                }

            }

            public void SetParam(string key, string value)
            {
                configParams.Add(key, value);
            }

            public string GetParam(string key)
            {
                if (!configParams.ContainsKey(key))
                    return null;
                return configParams[key];
            }

            public List<string> GetArray(string key)
            {
                if (!configArrays.ContainsKey(key))
                    return null;
                return configArrays[key];
            }

            public Dictionary<string,string> GetMap(string key)
            {
                if (!configMaps.ContainsKey(key))
                    return null;
                return configMaps[key];
            }
        }
    
    }


}
