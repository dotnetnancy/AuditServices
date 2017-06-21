using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using AuditServicesCommon;
using DotNetNancy.Core.Data;
using DataContractsAndProxy;
using log4net;

namespace AuditServicesDataAccess
{
    public class AuditDatabase
    {
        #region Properties
        AuditServicesDataAccess.CustomDatabaseHelper _databaseHelper = new CustomDatabaseHelper();
        ILog _log;
        SqlConnection _sqlConnection;
        #endregion

        #region Constructor
        public AuditDatabase()
        {
            //_log = log4net.LogManager.GetLogger(Constants.AUDIT_SERVICE_NAMED_LOGGER);
            _log = log4net.LogManager.GetLogger(Constants.AUDIT_SERVICE_NAMED_LOGGER);
            _sqlConnection = new SqlConnection(SqlHelper.Connections[0].ConnectionString);
        }
        #endregion

        public Guid WriteAudit(CustomObjectToAuditAuditPoint auditPoint)
        {
            Guid returnValue = Guid.Empty;

            //it is extremely important to sort the parameters in the parameters dictionary so that we can do group by
            //clauses in the sproc for easy finding of duplicate jobs.  just use the default sort in the sorted dictionary, asc i think
            //by key

            SortedDictionary<string, string> sortedParameters = new SortedDictionary<string, string>(auditPoint.Parameters);

            try
            {
                SqlDateTime auditDateTimeStampSql = (auditPoint.AuditDateTimeStamp == DateTime.MaxValue ||
                             auditPoint.AuditDateTimeStamp == DateTime.MinValue) ? SqlDateTime.MinValue : new System.Data.SqlTypes.SqlDateTime(auditPoint.AuditDateTimeStamp);

                string referenceID = (string.IsNullOrEmpty(auditPoint.ReferenceID)) ? null : auditPoint.ReferenceID;

                Guid ApplicationID = GetApplication(auditPoint.ApplicationName.ToString());
                Guid AuditCategoryID = GetAuditCategory(auditPoint.AuditingCategory.ToString());
                Guid OriginationID = GetOrigination(auditPoint.OriginationID.ToString());
                Guid StatusID = GetStatus(auditPoint.ReportedStatus.ToString());

                SqlParameter auditIDParam = SqlHelper.PrepareParameter("AuditID", Guid.NewGuid(),SqlDbType.UniqueIdentifier,ParameterDirection.InputOutput);
                SqlParameter auditCategoryIDParam = SqlHelper.PrepareParameter("AuditCategoryID", AuditCategoryID);
                SqlParameter auditCategoryNameParam = SqlHelper.PrepareParameter("AuditCategoryName", auditPoint.AuditingCategory.ToString());
                SqlParameter OriginationIDParam = SqlHelper.PrepareParameter("OriginationID", OriginationID);
                SqlParameter statusIDParam = SqlHelper.PrepareParameter("StatusID", StatusID);
                SqlParameter dateTimeStampParam = SqlHelper.PrepareParameter("DateTimeStamp", auditDateTimeStampSql);
                SqlParameter messageParam = SqlHelper.PrepareParameter("Message", auditPoint.Message);
                SqlParameter applicationIDParam = SqlHelper.PrepareParameter("ApplicationID", ApplicationID);
                SqlParameter referenceIDParam = SqlHelper.PrepareParameter("ReferenceID", referenceID);

                QueueHelper<SortedDictionary<string, string>> queueHelper = new QueueHelper<SortedDictionary<string, string>>();

                SqlParameter parametersDictionary =
                    SqlHelper.PrepareParameter("ParameterDictionary", queueHelper.GetStringMessage(sortedParameters));

                SqlParameter[] parameters = new SqlParameter[] { 
                  auditIDParam , 
                  auditCategoryIDParam ,
                  auditCategoryNameParam ,
                  OriginationIDParam,                  
                  statusIDParam ,
                  dateTimeStampParam ,
                  messageParam,                  
                  applicationIDParam,
                parametersDictionary,
                referenceIDParam};

                using (SqlDataReader reader = _databaseHelper.getDataReaderFromSP(_sqlConnection.ConnectionString,
                    DatabaseConstants.INSERT_AUDIT,
                 parameters))
                {
                    returnValue = (Guid)auditIDParam.Value;
                }

                //insert the individual parameters in addition to the dictionary of those parameters
                foreach (KeyValuePair<string, string> kvpParameters in auditPoint.Parameters)
                {
                    SqlParameter[] auditparams = new SqlParameter[] {SqlHelper.PrepareParameter("AuditID",(Guid)auditIDParam.Value)
                        , SqlHelper.PrepareParameter("ParamName", kvpParameters.Key),
                         SqlHelper.PrepareParameter("ParamValue", kvpParameters.Value)};

                    using (SqlDataReader reader = _databaseHelper.getDataReaderFromSP(_sqlConnection.ConnectionString,
                    DatabaseConstants.INSERT_AUDIT_PARAMETER,
                 auditparams))
                    {
                        returnValue = (Guid)auditIDParam.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Fatal(ex);
                throw new ApplicationException("Exception Trying to Write Audit Record to Database", ex);
            }

            return returnValue;
        }


        public Guid GetAuditCategory(string auditCategory)
        {
            SqlParameter auditCategoryIDParam = SqlHelper.PrepareParameter("AuditCategoryID", Guid.NewGuid(), SqlDbType.UniqueIdentifier, ParameterDirection.Output);
            SqlParameter auditCategoryNameParam = SqlHelper.PrepareParameter("AuditCategoryName", auditCategory);

            SqlParameter[] parameters = new SqlParameter[] { auditCategoryIDParam, auditCategoryNameParam };

            using (SqlDataReader reader = _databaseHelper.getDataReaderFromSP(_sqlConnection.ConnectionString,
                DatabaseConstants.GET_AUDIT_CATEGORY,
                parameters))
            {
                return (Guid)auditCategoryIDParam.Value;
            }
        }

        public Guid GetOrigination(string origination)
        {
            SqlParameter OriginationIDParam = SqlHelper.PrepareParameter("OriginationID", Guid.NewGuid(), SqlDbType.UniqueIdentifier, ParameterDirection.Output);
            SqlParameter OriginationNameParam = SqlHelper.PrepareParameter("OriginationName", origination);

            SqlParameter[] parameters = new SqlParameter[] { OriginationIDParam, OriginationNameParam };

            using (SqlDataReader reader = _databaseHelper.getDataReaderFromSP(_sqlConnection.ConnectionString,
                DatabaseConstants.GET_ORIGINATION,
                parameters))
            {
                return (Guid)OriginationIDParam.Value;
            }
        }

        public Guid GetStatus(string status)
        {
            SqlParameter StatusIDParam = SqlHelper.PrepareParameter("StatusID", Guid.NewGuid(), SqlDbType.UniqueIdentifier, ParameterDirection.Output);
            SqlParameter StatusNameParam = SqlHelper.PrepareParameter("StatusName", status);

            SqlParameter[] parameters = new SqlParameter[] { StatusIDParam, StatusNameParam };

            using (SqlDataReader reader = _databaseHelper.getDataReaderFromSP(_sqlConnection.ConnectionString,
                DatabaseConstants.GET_STATUS,
                parameters))
            {
                return (Guid)StatusIDParam.Value;
            }
        }

        public Guid GetApplication(string Application)
        {
            SqlParameter ApplicationIDParam = SqlHelper.PrepareParameter("ApplicationID", Guid.NewGuid(), SqlDbType.UniqueIdentifier, ParameterDirection.Output);
            SqlParameter ApplicationNameParam = SqlHelper.PrepareParameter("ApplicationName", Application);

            SqlParameter[] parameters = new SqlParameter[] { ApplicationIDParam, ApplicationNameParam };

            using (SqlDataReader reader = _databaseHelper.getDataReaderFromSP(_sqlConnection.ConnectionString,
                DatabaseConstants.GET_APPLICATION,
                parameters))
            {
                return (Guid)ApplicationIDParam.Value;
            }
        }


    }
}
