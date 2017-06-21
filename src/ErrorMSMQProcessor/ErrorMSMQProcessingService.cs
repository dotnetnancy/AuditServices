using System;
using System.Reflection;
using System.ServiceModel;
using log4net;
using AuditServicesCommon;

namespace DotNetNancyAuditErrorMSMQProcessingService
{
    //if you do not set the instancecontextmode.single then the constructor does not get called
    [ServiceBehavior(Namespace = "http://DotNetNancy.com/services/DotNetNancyAuditErrorMSMQProcessingService",
   InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class DotNetNancyAuditErrorMSMQProcessingService : IDotNetNancyAuditErrorMSMQProcessingService
    {
        ILog _log;
        public DotNetNancyAuditErrorMSMQProcessingService()
        {
            //_log = log4net.LogManager.GetLogger(Constants.AUDIT_SERVICE_NAMED_LOGGER);
            _log = log4net.LogManager.GetLogger(Constants.AUDIT_SERVICE_NAMED_LOGGER);
            _log.Debug("MSMQProcessingService constructor called");
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
            DotNetNancyAuditErrorMSMQImplementation implementation = new DotNetNancyAuditErrorMSMQImplementation();
            implementation.Start();
        }
    }
}

