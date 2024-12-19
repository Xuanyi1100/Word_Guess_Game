// Helpers/AppConfigManager.cs
using System;
using System.Configuration;

namespace try_to_build_client.Helpers
{
    public class AppConfigManager
    {
        public static string GetSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
        public static void SetSetting(string key, string value)
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            if (settings[key] == null)
            {
                settings.Add(key, value);
            }
            else
            {
                settings[key].Value = value;
            }
            configFile.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
        }
    }
}