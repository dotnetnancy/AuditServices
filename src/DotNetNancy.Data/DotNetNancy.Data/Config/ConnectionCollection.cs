using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace DotNetNancy.Core.Data.Config
{
    [ConfigurationCollection(typeof(Connection),
    CollectionType = ConfigurationElementCollectionType.BasicMap, AddItemName = "connection")]
    public class ConnectionCollection:ConfigurationElementCollection
    {
        private static ConfigurationPropertyCollection __properties;
        
        #region Constructors
        static ConnectionCollection()
        {
            __properties = new ConfigurationPropertyCollection();
        }

        public ConnectionCollection()
        {
        }
        #endregion

         #region Properties
        protected override ConfigurationPropertyCollection Properties
        {
            get { return __properties; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get
            {
                return "Connection";
            }
        }
        #endregion

        public void Remove( Connection element )
    {
        if( BaseIndexOf( element ) >= 0 )
        {
            BaseRemove( element.Name );
        }
    }

    public void RemoveAt( int index )
    {
        BaseRemoveAt( index );
    }
        #region Indexers
        public Connection this[int index]
        {
            get { return (Connection)base.BaseGet(index); }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                base.BaseAdd(index, value);
            }
        }

        public new Connection this[string name]
        {
            get { return (Connection)base.BaseGet(name); }
        }
        #endregion

        #region Overrides
        protected override ConfigurationElement CreateNewElement()
        {
            return new Connection();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as Connection).Name;
        }
        #endregion

        public override bool IsReadOnly()
        {
            return false;
        }

        protected override bool IsModified()
        {
            bool modified = true;
            return modified;
            
        }

        
    }
}


