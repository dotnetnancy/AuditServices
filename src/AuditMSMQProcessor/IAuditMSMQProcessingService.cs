using System.ServiceModel;
namespace AuditMSMQProcessingService
{
    [ServiceContract(Namespace = "http://DotNetNancy.com/services/AuditMSMQProcessingService")]
    public interface IAuditMSMQProcessingService
    {
        [OperationContract]
        void Start();
    }

}
