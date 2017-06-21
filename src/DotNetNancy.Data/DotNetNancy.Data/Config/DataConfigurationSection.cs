using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace DotNetNancy.Core.Data.Config
{
    public class DataConfigurationSection:ConfigurationSection
    {
        private static ConfigurationProperty __connections;

        private static ConfigurationPropertyCollection __properties;

        static DataConfigurationSection()
        {
            __connections = new ConfigurationProperty("Connections", typeof(ConnectionCollection), new ConnectionCollection(), ConfigurationPropertyOptions.IsRequired);

            __properties = new ConfigurationPropertyCollection();
            __properties.Add(__connections);
            
        }

        [ConfigurationProperty("Connections")]
        public ConnectionCollection Connections
        {
            get { return (ConnectionCollection)base[__connections]; }
        }

        public Connection GetDefaultConnection()
        {
            Connection returnVal=this.Connections[0];
            foreach (Connection c in this.Connections)
            {
                if (c.IsDefault)
                {
                    returnVal = c;
                    break;
                }
            }
            return returnVal;
        }

        internal void CheckEncrypted()
        {
            bool encrypted=true;
            for (int i = 0; i < Connections.Count; i++)
            {
                Connection c = Connections[i];
                if (!IsBase64String(c.Password))
                {
                    //c.Password = EncryptValue(c.Password);
                    encrypted = false;
                    break; //can jump out since the encrypt reprocesses each connection
                }
            }
            if (!encrypted)
            {
                AppDomain currentDomain = AppDomain.CurrentDomain;
                string configFile = currentDomain.SetupInformation.ConfigurationFile;
                ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
                fileMap.ExeConfigFilename=configFile;
                Configuration config=ConfigurationManager.OpenMappedExeConfiguration(fileMap,ConfigurationUserLevel.None);
                DataConfigurationSection section = (DataConfigurationSection)config.Sections["DotNetNancy.Data"];
                for (int i = 0; i < section.Connections.Count; i++)
                {
                    Connection c = Connections[i];
                    if (!IsBase64String(c.Password))
                    {
                        c.Password = EncryptValue(c.Password);                        
                    }
                    section.Connections[i] = c;
                }
                config.Save(ConfigurationSaveMode.Modified);

            }
        }

        private bool IsBase64String(string val)
        {
            bool returnVal = true;
            try
            {
                byte[] test = Convert.FromBase64String(val);
            }
            catch (Exception)
            {
                returnVal = false;
            }

            return returnVal;
        }

        private string EncryptValue(string inputString)
        {
            string returnVal = null;
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            byte[] unencryptedBytes = encoding.GetBytes(inputString);

            byte[] encrytpedBytes = System.Security.Cryptography.ProtectedData.Protect(unencryptedBytes, null, System.Security.Cryptography.DataProtectionScope.LocalMachine);

            returnVal = Convert.ToBase64String(encrytpedBytes);
            return returnVal;

        }

        private string DecryptValue(string base64string)
        {
            string returnVal = null;
            byte[] encrytpedBytes=Convert.FromBase64String(base64string);
            byte[] unencrytpedBytes=System.Security.Cryptography.ProtectedData.Unprotect(encrytpedBytes, null, System.Security.Cryptography.DataProtectionScope.LocalMachine);
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            returnVal=encoding.GetString(unencrytpedBytes);
            return returnVal;

        }

        public override bool IsReadOnly()
        {
            return false;
        }
        protected override bool IsModified()
        {
            return true;
        }

    }
}
