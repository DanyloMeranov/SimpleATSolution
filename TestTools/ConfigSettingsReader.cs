using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;


namespace TestTools
{
    public static class ConfigSettingsReader
    {
        private static AppSettingsSection _appSettingsSection;
        private static AppSettingsSection SettingsSection
        {
            get
            {
                if (_appSettingsSection == null)
                    Init();
                return _appSettingsSection;
            }
        }
        public static string BrowserName => SettingsSection.Settings["Browser"].Value;
        public static string AppSite => SettingsSection.Settings["ApplicationSite"].Value;
        public static string AppPort => SettingsSection.Settings["ApplicationPort"].Value;
        public static string Tenant => SettingsSection.Settings["Tenant"].Value;
        public static string TestUserName => SettingsSection.Settings["Username"].Value;
        public static string TestUserPass => SettingsSection.Settings["Password"].Value;
        public static string HubUrl => SettingsSection.Settings["HubUrl"].Value;
        public static byte DebugLvl => Convert.ToByte(SettingsSection.Settings["Debug"].Value);
        public static bool HeadLess => Convert.ToBoolean(SettingsSection.Settings["HeadLess"].Value);
        public static string DownloadPath => SettingsSection.Settings["DownloadPath"].Value;
        public static string ReportPath => SettingsSection.Settings["ReportPath"].Value;
        public static bool MultiUser => Convert.ToBoolean(SettingsSection.Settings["MultiUser"].Value);
        public static string RegionalSettings => SettingsSection.Settings["RegionalSet"].Value;

        public static int WaitForMinutes => Convert.ToInt32(SettingsSection.Settings["WaitForMinutes"].Value);

        public static int DefaultTimeOut()
        {
            int.TryParse(SettingsSection.Settings["DefaultTimeOut"].Value, out var intResult);
            return intResult;
        }

        public static void Init()
        {
            var fileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = ".\\app.config"
            };

            try
            {
                var appConfig = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                _appSettingsSection = appConfig.GetSection("appSettings") as AppSettingsSection;
            }
            catch (Exception e)
            {
                Assert.Fail($"Fail is happened with message: {e.Message}");
            }
        }
    }
}
