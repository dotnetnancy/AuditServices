using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AuditServicesDataAccess
{
    public static class DatabaseConstants
    {
        public const string AUDIT_DATABASE_CONNECTION = @"AuditDbConnection";
        public const string GET_AUDIT_CATEGORY = @"GetAuditCategory";
        public const string GET_ORIGINATION = @"GetOrigination";
        public const string GET_STATUS = @"GetStatus";
        public const string GET_APPLICATION = @"GetApplication";
        public const string INSERT_AUDIT = @"InsertAudit";
        public const string INSERT_AUDIT_PARAMETER = @"UpsertAuditParameters";
    }
}
