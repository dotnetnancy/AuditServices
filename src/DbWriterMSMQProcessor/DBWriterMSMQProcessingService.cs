using System;
using System.Reflection;
using System.ServiceModel;
using log4net;
using AuditServicesCommon;

namespace DotNetNancyAuditDbWriterMSMQProcessor
{
    //if you do not set the instancecontextmode.single then the constructor does not get called
    [ServiceBehavior(Namespace = "http://DotNetNancy.com/services/DotNetNancyAuditDBWriterMSMQProcessingService",
   InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class DotNetNancyAuditDBWriterDBWriterMSMQProcessingService : IDotNetNancyAuditDBWriterMSMQProcessingService
    {
        ILog _log;
        public DotNetNancyAuditDBWriterDBWriterMSMQProcessingService()
        {
            //_log = log4net.LogManager.GetLogger(Constants.AUDIT_SERVICE_NAMED_LOGGER);
            _log = log4net.LogManager.GetLogger(Constants.AUDIT_SERVICE_NAMED_LOGGER);

            _log.Debug("DBWriterMSMQProcessingService constructor called");

            try
            {
                Start();
            }
            catch (Exception ex)
            {
                _log.Fatal(ex);
            }
        }
        public void Start()
        {
            DotNetNancyAuditDBWriterImplementation implementation = new DotNetNancyAuditDBWriterImplementation();
            implementation.Start();
        }
    }
}

