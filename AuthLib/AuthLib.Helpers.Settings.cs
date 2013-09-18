using System.Linq;
using System.Web.Configuration;

namespace AuthLib.Helpers
{
    public static class Settings
    {
        private static  System.Configuration.Configuration config = ConfigurationManagement.GetConfiguration();

        /// <summary>
        /// Gets or sets a value indicating whether debug mode state.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is debug mode on; otherwise, <c>false</c>.
        /// </value>
        public static bool isDebugMode
        {
            get {
                if (config.AppSettings.Settings.AllKeys.Contains("Debug"))
                    return bool.Parse(config.AppSettings.Settings["Debug"].Value.ToString());
                else
                {
                   config.AppSettings.Settings.Add("Debug", "False");
                    return false;
                }
            }
            set
            {
                if (config.AppSettings.Settings.AllKeys.Contains("Debug"))
                    config.AppSettings.Settings["Debug"].Value = value.ToString();
                else
                {
                    config.AppSettings.Settings.Add("Debug", value.ToString());
                }
            }
        }


        public static string ApplicationName
        {
            get
            {
                if (config.AppSettings.Settings.AllKeys.Contains("ApplicationName"))
                    return config.AppSettings.Settings["ApplicationName"].Value.ToString();
                else
                {
                    config.AppSettings.Settings.Add("ApplicationName", "WebApplicationName");
                    return "WebApplicationName";
                }
            }
            set
            {
                if (config.AppSettings.Settings.AllKeys.Contains("ApplicationName"))
                    config.AppSettings.Settings["ApplicationName"].Value = value;
                else
                {
                    config.AppSettings.Settings.Add("ApplicationName", value);
                }
            }
        }

        public static string LogPath
        {
            get
            {
                if (!System.IO.Directory.Exists("C:\\logs")) System.IO.Directory.CreateDirectory("C:\\logs");
                if (config.AppSettings.Settings.AllKeys.Contains("LogPath"))
                    return config.AppSettings.Settings["LogPath"].Value.ToString();
                else
                {
                    config.AppSettings.Settings.Add("LogPath", "C:\\logs\\");
                    return "C:\\logs\\";
                }
            }
            set
            {
                if (!System.IO.Directory.Exists(value)) System.IO.Directory.CreateDirectory(value);
                if (config.AppSettings.Settings.AllKeys.Contains("LogPath"))
                {
                    config.AppSettings.Settings["LogPath"].Value = value;
                }
                else
                {
                    config.AppSettings.Settings.Add("LogPath", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the domain users base connection string.
        /// </summary>
        /// <value>
        /// The RIC base connection string.
        /// </value>
        public static string DomainUsersConnectionString
        {
            get
            {
                if (object.Equals(config.ConnectionStrings.ConnectionStrings["DomainUsersConnectionString"], null))
                    config.ConnectionStrings.ConnectionStrings.Add(new System.Configuration.ConnectionStringSettings
                    {
                        Name = "DomainUsersConnectionString",
                        ConnectionString = "Data Source=localhost;Initial Catalog=Users;Persist Security Info=True;User ID=sa; Password=123456789;",
                        ProviderName = "System.Data.SqlClient"
                    });
                return config.ConnectionStrings.ConnectionStrings["DomainUsersConnectionString"].ConnectionString;
            }
            set
            {
                if (object.Equals(config.ConnectionStrings.ConnectionStrings["DomainUsersConnectionString"], null))
                    config.ConnectionStrings.ConnectionStrings.Add(new System.Configuration.ConnectionStringSettings
                    {
                        Name = "RicBaseConnectionString",
                        ConnectionString = value,
                        ProviderName = "System.Data.SqlClient"
                    });
                config.ConnectionStrings.ConnectionStrings["DomainUsersConnectionString"].ConnectionString = value;
            }
        }

        /// <summary>
        /// Determines whether [is settings exists] [the specified key].
        /// </summary>
        /// <param name="Key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="createEntry">if set to <c>true</c> [create entry].</param>
        /// <returns>
        ///   <c>true</c> if [is settings exists] [the specified key]; otherwise, <c>false</c>.
        /// </returns>
        private static bool isSettingsExists(string Key, string value=null, bool createEntry = false)
        {
            if (!config.AppSettings.Settings.AllKeys.Contains(Key))
            {
                if (createEntry)
                    WebConfigurationManager.AppSettings.Add(Key, value);
                else
                    return false;
            }
            return true;
        }
          
        public static bool LoggingEnabled
        {
            get
            {
                if (config.AppSettings.Settings.AllKeys.Contains("enableLogging"))
                    return bool.Parse(config.AppSettings.Settings["enableLogging"].Value.ToString());
                else
                {
                    config.AppSettings.Settings.Add("enableLogging", "true");
                    return false;
                }
            }
            set
            {
                if (config.AppSettings.Settings.AllKeys.Contains("enableLogging"))
                    config.AppSettings.Settings["enableLogging"].Value = value.ToString();
                else
                {
                    config.AppSettings.Settings.Add("enableLogging", value.ToString());
                }
            }
        }



}
    public static class ConfigurationManagement
    {
        public static System.Configuration.Configuration GetConfiguration()
        {
            return System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(System.Web.HttpContext.Current.Request.ApplicationPath);
        }

        public static string GetConnectionString(string ConnectionStringName)
        {
            return GetConfiguration().ConnectionStrings.ConnectionStrings[ConnectionStringName].ConnectionString;
        }
    }

    public static class Types
    {
        public  enum UserType
        {
            DomainUser = 0
        }
    }

}
