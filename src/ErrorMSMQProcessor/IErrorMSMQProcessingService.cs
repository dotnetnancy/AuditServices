using System.ServiceModel;
namespace DotNetNancyAuditErrorMSMQProcessingService
{
    [ServiceContract(Namespace = "http://DotNetNancy.com/services/DotNetNancyAuditErrorMSMQProcessingService")]
    public interface IDotNetNancyAuditErrorMSMQProcessingService
    {
        [OperationContract]
        void Start();
    }

}
