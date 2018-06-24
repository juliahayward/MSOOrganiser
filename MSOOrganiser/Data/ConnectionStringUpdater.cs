using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MSOOrganiser.Data
{
    // Add new connection strings to remote installations
    public static class ConnectionStringUpdater
    {
        public static void Update()
        {
            string appPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string configFile = System.IO.Path.Combine(appPath, "MSOOrganiser.exe.config");
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
            configFileMap.ExeConfigFilename = configFile;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, 
                ConfigurationUserLevel.None);

            // Old format
            if (config.ConnectionStrings.ConnectionStrings["Production"] == null)
            {
                var test = config.ConnectionStrings.ConnectionStrings["DataEntities"];
                var prod = test.ConnectionString.Replace("**REDACTEDAwsDbName**", "**REDACTEDAwsDbName**_prod");
                config.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings()
                {
                    ConnectionString = prod,
                    Name = "Production",
                    ProviderName = test.ProviderName
                });
                config.Save();
            }
        }
    }
}
