using System;
using System.IO;
using UnityEngine;
using ITCompanySimulation.Core;

namespace ITCompanySimulation.Settings
{
    /// <summary>
    /// Class used for serializing configuration files in JSON format.
    /// </summary>
    public class ConfigFileManager : IObjectStorage
    {
        /*Private consts fields*/

        /*Private fields*/

        private string ConfigFileDirectoryPath;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        /*Public methods*/

        public void DeserializeObject(object objectToDeserializeInstance, string configFileName)
        {
            string configFilePath = Path.Combine(ConfigFileDirectoryPath, configFileName);

            try
            {
                using (FileStream configFileStream = File.Open(configFilePath, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader configFileStreamReader = new StreamReader(configFileStream))
                    {
                        string jsonString = configFileStreamReader.ReadToEnd();
                        JsonUtility.FromJsonOverwrite(jsonString, objectToDeserializeInstance);
                    }
                }
            }
            catch (Exception ex)
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                Debug.LogWarningFormat("[{0}] Could not read config file\n" +
                                       "File path: {1}\n" +
                                       "Error: {2}",
                                       this.GetType().Name,
                                       configFilePath,
                                       ex.Message);
#endif
            }
        }

        public void SerializeObject(object objectToSerialize, string configFileName)
        {
            string configFilePath = Path.Combine(ConfigFileDirectoryPath, configFileName);

            try
            {
                using (FileStream configFileStream = File.Open(configFilePath, FileMode.Create, FileAccess.Write))
                {
                    using (StreamWriter configFileStreamWriter = new StreamWriter(configFileStream))
                    {
                        string jsonString = JsonUtility.ToJson(objectToSerialize);
                        configFileStreamWriter.Write(jsonString);
                    }
                }
            }
            catch (Exception ex)
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                Debug.LogWarningFormat("[{0}] Could not write config file\n" +
                                       "File path: {1}\n" +
                                       "Error: {2}",
                                       this.GetType().Name,
                                       configFilePath,
                                       ex.Message);
#endif
            }

        }

        public ConfigFileManager(string configFileDirectoryPath)
        {
            this.ConfigFileDirectoryPath = configFileDirectoryPath;

            try
            {
                Directory.CreateDirectory(this.ConfigFileDirectoryPath);

            }
            catch (Exception ex)
            {
                Debug.LogWarningFormat("[{0}] Could not create config directory\n" +
                                       "Directory path: {1}\n" +
                                       "Error: {2}",
                                       this.GetType().Name,
                                       this.ConfigFileDirectoryPath,
                                       ex.Message);
            }
        }
    }
}
