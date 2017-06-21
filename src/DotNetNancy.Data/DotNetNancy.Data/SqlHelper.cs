// ===============================================================================
// Microsoft Data Access Application Block for .NET
// http://msdn.microsoft.com/library/en-us/dnbda/html/daab-rm.asp
//
// SQLHelper.cs
//
// This file contains the implementations of the SqlHelper and SqlHelperParameterCache
// classes.
//
// For more information see the Data Access Application Block Implementation Overview. 
// ===============================================================================
// Release history
// VERSION	DESCRIPTION
//   2.0	Added support for FillDataset, UpdateDataset and "Param" helper methods
//
// ===============================================================================
// Copyright (C) 2000-2001 Microsoft Corporation
// All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR
// FITNESS FOR A PARTICULAR PURPOSE.
// ==============================================================================

using System;
using System.Data;
using System.Xml;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using log4net;

namespace DotNetNancy.Core.Data
{
	/// <summary>
	/// The SqlHelper class is intended to encapsulate high performance, scalable best practices for 
	/// common uses of SqlClient
	/// </summary>
	public partial class SqlHelper
	{
		#region private utility methods & constructors
		private static Config.DataConfigurationSection _dbConnections;
		private static ILog _log =LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);
		private static int _commandTimeout;
		
		static SqlHelper() 
		{
			try 
			{
				_commandTimeout=30;  //implement the default value 30 seconds
                _dbConnections = (Config.DataConfigurationSection)System.Configuration.ConfigurationManager.GetSection("DotNetNancy.Data");
                _dbConnections.CheckEncrypted();
			}
			catch  (Exception ex)
			{
				_log.Error("Unable to read Config Section DotNetNancy.Data",ex);
				_dbConnections=null;
			}
		}

		// Since this class provides only static methods, make the default constructor private to prevent 
		// instances from being created with "new SqlHelper()"
		private SqlHelper() {}

		/// <summary>
		/// This method is used to attach array of SqlParameters to a SqlCommand.
		/// 
		/// This method will assign a value of DbNull to any parameter with a direction of
		/// InputOutput and a value of null.  
		/// 
		/// This behavior will prevent default values from being used, but
		/// this will be the less common case than an intended pure output parameter (derived as InputOutput)
		/// where the user provided no input value.
		/// </summary>
		/// <param name="command">The command to which the parameters will be added</param>
		/// <param name="commandParameters">An array of SqlParameters to be added to command</param>
		private static void AttachParameters(SqlCommand command, SqlParameter[] commandParameters)
		{
			if( command == null ) throw new ArgumentNullException( "command" );
			if( commandParameters != null )
			{
				foreach (SqlParameter p in commandParameters)
				{
					if( p != null )
					{
						// Check for derived output value with no value assigned
						if ( ( p.Direction == ParameterDirection.InputOutput || 
							p.Direction == ParameterDirection.Input ) && 
							(p.Value == null))
						{
							p.Value = DBNull.Value;
						}
						command.Parameters.Add(p);
					}
				}
			}
		}

		/// <summary>
		/// This method assigns dataRow column values to an array of SqlParameters
		/// </summary>
		/// <param name="commandParameters">Array of SqlParameters to be assigned values</param>
		/// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values</param>
		private static void AssignParameterValues(SqlParameter[] commandParameters, DataRow dataRow)
		{
			if ((commandParameters == null) || (dataRow == null)) 
			{
				// Do nothing if we get no data
				return;
			}

			int i = 0;
			// Set the parameters values
			foreach(SqlParameter commandParameter in commandParameters)
			{
				// Check the parameter name
				if( commandParameter.ParameterName == null || 
					commandParameter.ParameterName.Length <= 1 )
					throw new Exception( 
						string.Format( 
						"Please provide a valid parameter name on the parameter #{0}, the ParameterName property has the following value: '{1}'.", 
						i, commandParameter.ParameterName ) );
				if (dataRow.Table.Columns.IndexOf(commandParameter.ParameterName.Substring(1)) != -1)
					commandParameter.Value = dataRow[commandParameter.ParameterName.Substring(1)];
				i++;
			}
		}

		/// <summary>
		/// This method assigns an array of values to an array of SqlParameters
		/// </summary>
		/// <param name="commandParameters">Array of SqlParameters to be assigned values</param>
		/// <param name="parameterValues">Array of objects holding the values to be assigned</param>
		private static void AssignParameterValues(SqlParameter[] commandParameters, object[] parameterValues)
		{
			if ((commandParameters == null) || (parameterValues == null)) 
			{
				// Do nothing if we get no data
				return;
			}

			// We must have the same number of values as we pave parameters to put them in
			if (commandParameters.Length != parameterValues.Length)
			{
				throw new ArgumentException("Parameter count does not match Parameter Value count.");
			}

			// Iterate through the SqlParameters, assigning the values from the corresponding position in the 
			// value array
			for (int i = 0, j = commandParameters.Length; i < j; i++)
			{
				// If the current array value derives from IDbDataParameter, then assign its Value property
				if (parameterValues[i] is IDbDataParameter)
				{
					IDbDataParameter paramInstance = (IDbDataParameter)parameterValues[i];
					if( paramInstance.Value == null )
					{
						commandParameters[i].Value = DBNull.Value; 
					}
					else
					{
						commandParameters[i].Value = paramInstance.Value;
					}
				}
				else if (parameterValues[i] == null)
				{
					commandParameters[i].Value = DBNull.Value;
				}
				else
				{
					commandParameters[i].Value = parameterValues[i];
				}
			}
		}

		/// <summary>
		/// This method opens (if necessary) and assigns a connection, transaction, command type and parameters 
		/// to the provided command
		/// </summary>
		/// <param name="command">The SqlCommand to be prepared</param>
		/// <param name="connection">A valid SqlConnection, on which to execute this command</param>
		/// <param name="transaction">A valid SqlTransaction, or 'null'</param>
		/// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">The stored procedure name or T-SQL command</param>
		/// <param name="commandParameters">An array of SqlParameters to be associated with the command or 'null' if no parameters are required</param>
		/// <param name="mustCloseConnection"><c>true</c> if the connection was opened by the method, otherwose is false.</param>
		private static void PrepareCommand(SqlCommand command, SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, SqlParameter[] commandParameters, out bool mustCloseConnection )
		{
			if( command == null ) throw new ArgumentNullException( "command" );
			if( commandText == null || commandText.Length == 0 ) throw new ArgumentNullException( "commandText" );

			// If the provided connection is not open, we will open it
			if (connection.State != ConnectionState.Open)
			{
				mustCloseConnection = true;
				connection.Open();
			}
			else
			{
				mustCloseConnection = false;
			}
			
			// Attach the command parameters if they are provided
			if (commandParameters != null)
			{
				AttachParameters(command, commandParameters);
			}

			// Associate the connection with the command
			command.Connection = connection;

			// Set the command text (stored procedure name or SQL statement)
			command.CommandText = commandText;

			// If we were provided a transaction, assign it
			if (transaction != null)
			{
				if( transaction.Connection == null ) throw new ArgumentException( "The transaction was rolled backed or commited, please provide an open transaction.", "transaction" );
				command.Transaction = transaction;
			}

			// Set the command type
			command.CommandType = commandType;

			command.CommandTimeout=_commandTimeout;

			
			return;
		}

		#endregion private utility methods & constructors

		#region CreateCommand
		/// <summary>
		/// Simplify the creation of a Sql command object by allowing
		/// a stored procedure and optional parameters to be provided
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  SqlCommand command = CreateCommand(conn, "AddCustomer", "CustomerID", "CustomerName");
		/// </remarks>
		/// <param name="connection">A valid SqlConnection object</param>
		/// <param name="spName">The name of the stored procedure</param>
		/// <param name="sourceColumns">An array of string to be assigned as the source columns of the stored procedure parameters</param>
		/// <returns>A valid SqlCommand object</returns>
		public static SqlCommand CreateCommand(SqlConnection connection, string spName, params string[] sourceColumns) 
		{
			if( connection == null ) throw new ArgumentNullException( "connection" );
			if( spName == null || spName.Length == 0 ) throw new ArgumentNullException( "spName" );

			// Create a SqlCommand
			SqlCommand cmd = new SqlCommand( spName, connection );
			cmd.CommandType = CommandType.StoredProcedure;

			// If we receive parameter values, we need to figure out where they go
			if ((sourceColumns != null) && (sourceColumns.Length > 0)) 
			{
				// Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

				// Assign the provided source columns to these parameters based on parameter order
				for (int index=0; index < sourceColumns.Length; index++)
					commandParameters[index].SourceColumn = sourceColumns[index];

				// Attach the discovered parameters to the SqlCommand object
				AttachParameters (cmd, commandParameters);
			}

			return cmd;
		}
		#endregion
		
		#region PrepareParameter
		/// <summary>
		/// Prepares a SqlParameter to be used in subsequent command calls.
		/// </summary>
		/// <param name="name">The parameter name</param>
		/// <param name="val">The parameter value</param>
		/// <returns>A SqlParameter containing the options specified</returns>
		public static SqlParameter PrepareParameter(string name,object val)
		{
			if (name.Substring(0,1)!="@") 
			{
				name="@"+name;
			}
			SqlParameter param=new SqlParameter(name,val);
			param.Direction=ParameterDirection.Input;
			return param;
		}

		/// <summary>
		/// Prepares a SqlParameter to be used in subsequent command calls.
		/// </summary>
		/// <param name="name">The parameter name</param>
		/// <param name="val">The parameter value</param>
		/// <param name="paramType">The SqlDbType of the parameter</param>
		/// <returns>A SqlParameter containing the options specified</returns>
		public static SqlParameter PrepareParameter(string name,object val,System.Data.SqlDbType paramType)
		{
			return PrepareParameter(name,val,paramType,ParameterDirection.Input);			
		}

		/// <summary>
		/// Prepares a SqlParameter to be used in subsequent command calls.
		/// </summary>
		/// <param name="name">The parameter name</param>
		/// <param name="val">The parameter value</param>
		/// <param name="paramType">The SqlDbType of the parameter</param>
		/// <param name="paramDirection">The parameter direction</param>
		/// <returns>A SqlParameter containing the options specified</returns>
		public static SqlParameter PrepareParameter(string name,object val,SqlDbType paramType,ParameterDirection paramDirection)
		{
			if (name.Substring(0,1)!="@") 
			{
				name="@"+name;
			}
			SqlParameter param=PrepareParameter(name,val);					
			param.SqlDbType=paramType;
			param.Direction=paramDirection;
			return param;
		}
		#endregion

		#region Static Properties

		/// <summary>
		/// Returns the collection of configured Connections for the current application
		/// </summary>
		public static List<Config.Connection> Connections 
		{
			get 
			{
                List<Config.Connection> returnVal = new List<DotNetNancy.Core.Data.Config.Connection>();
                foreach (Config.Connection conn in _dbConnections.Connections)
                {
                    returnVal.Add(conn);
                }
                                                          
				return returnVal;
			}
		}

		public static int CommandTimeout
		{
			get
			{
				return _commandTimeout;
			}
			set 
			{
				if (value<0) 
				{
					throw new ArgumentException("CommandTimeout must be a positive integer");
				}
				_commandTimeout=value;
			}
		}
		#endregion

	
        public static Config.Connection GetDefaultConnection() 
        {
            return _dbConnections.GetDefaultConnection();
        }

        public static Config.Connection GetNamedConnection(string name)
        {
            return _dbConnections.Connections[name];
        }

         

	}


}

