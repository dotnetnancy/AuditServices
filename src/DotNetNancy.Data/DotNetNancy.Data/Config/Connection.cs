using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace DotNetNancy.Core.Data.Config
{
    public class Connection:ConfigurationElement
    {

        private static ConfigurationProperty __name;
        private static ConfigurationProperty __default;
        private static ConfigurationProperty __server;
        private static ConfigurationProperty __database;
        private static ConfigurationProperty __security;
        private static ConfigurationProperty __additional;
        private static ConfigurationProperty __username;
        private static ConfigurationProperty __password;
        private static ConfigurationProperty __useIntegrated;

        private static ConfigurationPropertyCollection __properties;

        private bool _modified;

                        #region Constructors
        /// <summary>
        /// Predefines the valid properties and prepares
        /// the property collection.
        /// </summary>
        static Connection()
        {
            __properties = new ConfigurationPropertyCollection();

            // Predefine properties here
            __name = new ConfigurationProperty(
                "name",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired
            );

            __default=new ConfigurationProperty(
                "default",
                typeof(bool),
                false,
                ConfigurationPropertyOptions.None
            );

            __server=new ConfigurationProperty(
                "server",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired
            );

             __database=new ConfigurationProperty(
                "dbname",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired
            );

             __security=new ConfigurationProperty(
                "security",
                typeof(string),
                null,
                ConfigurationPropertyOptions.None
            );
             __additional = new ConfigurationProperty(
                "additionalSettings",
                typeof(string),
                null,
                ConfigurationPropertyOptions.None
            );

             __username = new ConfigurationProperty(
                "username",
                typeof(string),
                "",
                ConfigurationPropertyOptions.None
            );
             __password  = new ConfigurationProperty(
                "password",
                typeof(string),
                "",
                ConfigurationPropertyOptions.None
            );
             __useIntegrated = new ConfigurationProperty(
                "integratedAuth",
                typeof(bool),
                false,
                ConfigurationPropertyOptions.None
            );

            
            __properties.Add(__name);
            __properties.Add(__default);
            __properties.Add(__server);
            __properties.Add(__database);
            __properties.Add(__security);
            __properties.Add(__additional);
            __properties.Add(__username);
            __properties.Add(__password);
            __properties.Add(__useIntegrated);
        }

        public Connection() { _modified = false; }
        #endregion
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)base[__name]; }
        }
        [ConfigurationProperty("default", IsRequired = false)]
        public bool IsDefault
        {
            get { return (bool)base[__default]; }
        }

        [ConfigurationProperty("server", IsRequired = true)]
        public string ServerName
        {
            get { return (string)base[__server]; }
        }
        [ConfigurationProperty("dbname", IsRequired = true)]
        public string DbName
        {
            get { return (string)base[__database]; }
        }
        [ConfigurationProperty("security", IsRequired = true)]
        public string SecurityString
        {
            get { return (string)base[__security]; }
        }

        [ConfigurationProperty("additionalSettings", IsRequired = false)]
        public string AdditionalSettings
        {
            get { return (string)base[__additional]; }
        }

        [ConfigurationProperty("username", IsRequired = false)]
        public string Username
        {
            get { return (string)base[__username]; }
        }

        [ConfigurationProperty("password", IsRequired = false)]
        public string Password
        {
            get { return (string)base["password"]; }
            set { 
                base["password"] = value;
                _modified = true;
            }
        }

        [ConfigurationProperty("integratedAuth", IsRequired = false)]
        public bool IntegratedAuth
        {
            get { return (bool)base[__useIntegrated]; }
        }



        public string ConnectionString
        {
            get 
            {
                string returnVal = null;
                if (IntegratedAuth)
                {
                    returnVal = string.Format("Data Source={0};Initial Catalog={1};Integrated Security=SSPI;{2}", this.ServerName, this.DbName, this.AdditionalSettings);
                }
                else
                {
                    if (Username != null && Username.Length > 0)
                    {
                        returnVal = string.Format("Data Source={0};Initial Catalog={1};user id={2};password={3};{4}", this.ServerName, this.DbName, this.Username, DecryptValue(this.Password), this.AdditionalSettings);
                    }
                    else
                    {
                        if (SecurityString != null && SecurityString.Length > 0)
                        {
                            returnVal = string.Format("Data Source={0};Initial Catalog={1};{2};{3}", this.ServerName, this.DbName, this.SecurityString, this.AdditionalSettings);
                        }
                        else
                        {
                            //config issue, raise an exception
                            throw new ConfigurationErrorsException("Invalid configuration values for DotNetNancy.Data.Connection");
                        }
                    }
                }
                return returnVal;
            
            }
        }


        /// <summary>
        /// Override the Properties collection and return our custom one.
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get { return __properties; }
        }

        public override bool IsReadOnly()
        {
            return false;
        }
        protected override bool IsModified()
        {
            return _modified;
        }

        private string DecryptValue(string base64string)
        {
            string returnVal = null;
            byte[] encrytpedBytes = Convert.FromBase64String(base64string);
            byte[] unencrytpedBytes = System.Security.Cryptography.ProtectedData.Unprotect(encrytpedBytes, null, System.Security.Cryptography.DataProtectionScope.LocalMachine);
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            returnVal = encoding.GetString(unencrytpedBytes);
            return returnVal;

        }
        
    }
}
