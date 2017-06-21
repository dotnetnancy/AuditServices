using System;
using System.Data;
using System.Xml;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using log4net;

namespace DotNetNancy.Core.Data
{
    partial class SqlHelper
    {
        #region Transactions

        /// <summary>
        /// Begins a Transaction on a new connection to the default database.
        /// </summary>
        /// <returns>A new open SqlTransaction</returns>
        public static SqlTransaction GetNewTransaction()
        {
            return GetNewTransaction(_dbConnections.GetDefaultConnection().ConnectionString);
        }

        /// <summary>
        /// Begins a Transaction on a new connection to the specified database.
        /// </summary>
        /// <param name="connString">The connection string to the database</param>
        /// <returns>SqlTransaction</returns>
        public static SqlTransaction GetNewTransaction(string connString)
        {
            SqlConnection connection = new SqlConnection(connString);
            connection.Open();
            SqlTransaction trans = connection.BeginTransaction();
            return trans;
        }

        /// <summary>
        /// Begins a Transaction on a new connection to the specified database.
        /// </summary>
        /// <param name="connSettings">The database settings object to use</param>
        /// <returns>A new open SqlTransaction</returns>
        public static SqlTransaction GetNewTransaction(Config.Connection connSettings)
        {
            return GetNewTransaction(connSettings.ConnectionString);
        }



        #endregion

    }
}
