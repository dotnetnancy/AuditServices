using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Messaging;
using System.Configuration;
using Proxy;
using AuditServicesCommon;
using System.ServiceModel;
using DataContractsAndProxy;

namespace Consumer
{
    class Program
    {

        static ILog _log;
        static void Main(string[] args)
        {
            _log = log4net.LogManager.GetLogger(Constants.AUDIT_SERVICE_NAMED_LOGGER);

            //System.Timers.Timer _t = new System.Timers.Timer(300);
            //_t.Elapsed += new System.Timers.ElapsedEventHandler(_t_Elapsed);

            //_t.Start();
            //_t_Elapsed(null, null);
            int count = 10000;

            PutStaticNumberOfMessages(count);

            Console.ReadLine();
        }

        static void PutStaticNumberOfMessages(int count)
        {
            try
            {
                for (int i = 0; i < count; i++)
                {
                    AuditServiceClient auditSvc = new AuditServiceClient();
                    Proxy.CustomObjectToAuditAuditPoint CustomObjectToAuditAuditPoint = null;

                    try
                    {

                        CustomObjectToAuditAuditPoint =
                        new Proxy.CustomObjectToAuditAuditPoint();

                        CustomObjectToAuditAuditPoint.AuditingCategory = AuditCategory.QueueList;
                        CustomObjectToAuditAuditPoint.ApplicationName = ApplicationName.SomeAutomatedJob;
                        CustomObjectToAuditAuditPoint.AuditDateTimeStamp = DateTime.UtcNow;
                        CustomObjectToAuditAuditPoint.OriginationID = Proxy.OriginationID.Undefined;
                        CustomObjectToAuditAuditPoint.Message = "Included in a Queue List for Queue Mine Job";

                        //made this a varying set of parameters that are added to the CustomObjectToAuditAuditPoint
                        Dictionary<string, string> parameters = new Dictionary<string, string>();
                        parameters.Add("QueueNumber", "123");
                        parameters.Add("QueueCategory", "345");
                        parameters.Add("RecordIdentifier", "ABC123");
                        CustomObjectToAuditAuditPoint.Parameters = parameters;

                        CustomObjectToAuditAuditPoint.ReportedStatus = Status.Success;
                        CustomObjectToAuditAuditPoint.ReferenceID = Guid.NewGuid().ToString();

                        auditSvc.SubmitAudit(GetAuditPoint(CustomObjectToAuditAuditPoint));
                        Console.WriteLine("Submitted Audit Point to Audit Service");
                    }
                    catch (TimeoutException te)
                    {
                        Console.WriteLine(te.StackTrace);

                    }
                    catch (FaultException fe)
                    {
                        Console.WriteLine(fe.StackTrace);
                    }
                    catch (CommunicationException ce)
                    {
                        Console.WriteLine(ce.StackTrace);
                    }

                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.StackTrace);
                    }

                    finally
                    {
                        auditSvc.CloseProxy();
                        CustomObjectToAuditAuditPoint = null;
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.StackTrace);
            }

        }

        private static DataContractsAndProxy.CustomObjectToAuditAuditPoint GetAuditPoint(Proxy.CustomObjectToAuditAuditPoint customObjectToAuditAuditPoint)
        {
            throw new NotImplementedException("Use whichever library like Automapper or something to implement this conversion from proxy to service contract," +
                "ideally there would be a service reference that defines the proxy, this just shows the difference between the proxy and service implementation contract");
        }

        static void _t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                AuditServiceClient client = new AuditServiceClient();

                Proxy.CustomObjectToAuditAuditPoint CustomObjectToAuditAuditPoint =
                 new Proxy.CustomObjectToAuditAuditPoint();

                CustomObjectToAuditAuditPoint.AuditingCategory = AuditCategory.QueueList;
                CustomObjectToAuditAuditPoint.ApplicationName = ApplicationName.SomeAutomatedJob;
                CustomObjectToAuditAuditPoint.AuditDateTimeStamp = DateTime.Now;
                CustomObjectToAuditAuditPoint.OriginationID = Proxy.OriginationID.Undefined;
                CustomObjectToAuditAuditPoint.Message = "Included in a Queue List for Queue Mine Job";

                //made this a varying set of parameters that are added to the CustomObjectToAuditAuditPoint
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("QueueNumber", "123");
                parameters.Add("QueueCategory", "345");
                parameters.Add("RecordIdentifier", "ABC123");
                CustomObjectToAuditAuditPoint.Parameters = parameters;
                CustomObjectToAuditAuditPoint.Parameters = parameters;

                CustomObjectToAuditAuditPoint.ReportedStatus = Status.Success;
                CustomObjectToAuditAuditPoint.ReferenceID = Guid.NewGuid().ToString();

                client.SubmitAudit(GetAuditPoint(CustomObjectToAuditAuditPoint));
                Console.WriteLine("Submitted Audit Point to Audit Service");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
