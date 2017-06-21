using System.ServiceModel;
namespace DotNetNancyAuditDbWriterMSMQProcessor
{
    [ServiceContract(Namespace = "http://DotNetNancy.com/services/DotNetNancyAuditDBWriterMSMQProcessingService")]
    public interface IDotNetNancyAuditDBWriterMSMQProcessingService
    {
        [OperationContract]
        void Start();
    }

}
