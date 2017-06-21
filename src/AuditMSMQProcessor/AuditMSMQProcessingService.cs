using System;
using System.Reflection;
using System.ServiceModel;
using log4net;
using AuditServicesCommon;

namespace AuditMSMQProcessingService
{
    //if you do not set the instancecontextmode.single then the constructor does not get called
    [ServiceBehavior(Namespace = "http://DotNetNancy.com/services/AuditMSMQProcessingService",
   InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class AuditMSMQProcessingService : IAuditMSMQProcessingService
    {
        ILog _log;
        public AuditMSMQProcessingService()
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
            AuditMSMQImplementation implementation = new AuditMSMQImplementation();
            implementation.Start();
        }
    }
}

