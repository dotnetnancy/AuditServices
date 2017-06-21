using DataContractsAndProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;


namespace AuditingService
{
    [ServiceContract(Namespace = "http://DotNetNancy.com/services/AuditService")]
    public interface IAuditService
    {
        [OperationContract]
        string SubmitAudit(CustomObjectToAuditAuditPoint auditPoint);
    }

}
