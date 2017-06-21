using System;
using System.Configuration;
using System.Linq;
using System.Messaging;
using System.Reflection;
using AuditServicesCommon;
using DataContractsAndProxy;
using log4net;

namespace DotNetNancyAuditDbWriterMSMQProcessor
{
    public class DotNetNancyAuditDbWriterProcessor
    {
        #region Properties
        MessageWrapper _message;
        static ILog _log;
        static string _errorQueueName;
        static MessagePropertyFilter _countFilter = new MessagePropertyFilter();
        #endregion

        #region Construector
        static DotNetNancyAuditDbWriterProcessor()
        {
            //_log = log4net.LogManager.GetLogger(Constants.AUDIT_SERVICE_NAMED_LOGGER);
            _log = log4net.LogManager.GetLogger(Constants.AUDIT_SERVICE_NAMED_LOGGER);
            string messageQueueSetting = ConfigurationManager.AppSettings["errorQueueName"];
            _errorQueueName = messageQueueSetting == null ? "no message queue defined in config" : messageQueueSetting;

            _countFilter.AdministrationQueue = false;
            _countFilter.ArrivedTime = false;
            _countFilter.CorrelationId = false;
            _countFilter.Priority = false;
            _countFilter.ResponseQueue = false;
            _countFilter.SentTime = false;
            _countFilter.Body = false;
            _countFilter.Label = false;
            _countFilter.Id = false;
        }

        public DotNetNancyAuditDbWriterProcessor(DataContractsAndProxy.MessageWrapper Message)
        {
            try
            {
                _message = Message;
            }
            catch (Exception ex)
            {
                _log.Fatal(ex);
            }
        }
        #endregion

        #region Public Methods
        public object ThreadPoolCallback(Object ThreadContext)
        {
            _log.Debug("Message Successfully Received By DBWriterMSMQProcessor");

            WriteToDatabase();
            return 1;
        }
        #endregion

        #region Private Helper Methods
        private void WriteToDatabase()
        {
            try
            {
                _log.Info("Attempting to Write Message To Audit Central Store Database");

                WriteMessageToDatabase(_message.WrappedCustomObjectToAuditAuditPoint);

                _log.Info("Attempt to Write Message to Audit Central Store Database - Successful");
            }
            catch (Exception exDbDown)
            {
                _log.Fatal(exDbDown);
                _log.Info("Attempt to Write Message to Audit Central Store Database - UnSuccessful");
                try
                {
                    _log.Info("Attempting to Write Audit Message To Error MSMQ, Audit Central Store Database is Down in This Scenario");
                    DatabaseDownMitigation();
                    _log.Info("Write Audit Message To Error MSMQ - Successful");
                }
                catch (Exception exErrorMSMQDown)
                {
                    _log.Info("Write Audit Message To Error MSMQ - UnSuccessful");
                    _log.Fatal(exErrorMSMQDown);
                    try
                    {
                        _log.Info("Attempt to write Audit Payload Message to Log File - Multiple System Failure - Cannot Write Audit Point Message to the Database or the Error MSMQ" );
                        AllAlternateMitigationPathsFailure(_message.WrappedCustomObjectToAuditAuditPoint);
                        _log.Info("Attempt to write Audit Payload Message to Log File - Multiple System Failure - Log File Written - Successful - this log is the only place that this Audit Message Can now Be Found");

                    }
                    catch (Exception ex)
                    {
                        //nothing else can be done here
                        _log.Info("Attempt to write Audit Payload Message to Log File - Multiple System Failure - Log File Written - Unsuccessful - Audit Message Payload Lost");
                        _log.Fatal("Attempt to write Audit Payload Message to Log File - Multiple System Failure - Log File Written - Unsuccessful - Audit Message Payload Lost", ex);

                    }

                }
            }
        }

        public void AllAlternateMitigationPathsFailure(CustomObjectToAuditAuditPoint CustomObjectToAuditAuditPoint)
        {
            //clear the CustomObjectToAudit audit point from the dictionary
            //log a fatal         

            _log.Fatal("Attempts to Write Audit Point to Central Audit Database has Failed, Multiple System Failure");
            QueueHelper<CustomObjectToAuditAuditPoint> queueHelper = new QueueHelper<CustomObjectToAuditAuditPoint>();
            _log.Fatal("Audit Point Message That was Not Able to Be Processed:  " + queueHelper.GetStringMessage(CustomObjectToAuditAuditPoint));

        }

        private void WriteMessageToDatabase(CustomObjectToAuditAuditPoint auditPoint)
        {
            AuditServicesDataAccess.AuditDatabase _db = new AuditServicesDataAccess.AuditDatabase();
            _db.WriteAudit(auditPoint);
        }

        private string DatabaseDownMitigation()
        {
            string returnMessage = "Audit Point Received - Unsuccessfully";

            string messageThresholdString = ConfigurationManager.AppSettings["messageQueueThreshold"];
            int messageThreshold = Convert.ToInt32((messageThresholdString == null) ? "1000" : messageThresholdString);

            //this is our last ditch effort to keep this audit , we put it on an error
            //Q which then should be processed when the database is back up.            string queuePath = ConfigurationManager.AppSettings["dbWriterQueueName"];

            MessageQueue q = null;

            try
            {

                if (!MessageQueue.Exists(_errorQueueName))
                    MessageQueue.Create(_errorQueueName, true);

                q = new MessageQueue(_errorQueueName);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Cannot instantiate or create MSMQ " + _errorQueueName, ex);
            }
            finally
            {
                if (q != null)
                    q.Dispose();
            }

            try
            {
                if (IsMSMQOverThreshold(q, messageThreshold))
                    _log.Fatal("Audit MSMQ Message Threshold Exceeded, Please Check MSMQ " + q.QueueName);

                try
                {
                    WriteMessage(q, _message);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Cannot Write Messages to MSMQ " + _errorQueueName, ex);
                }
            }
            finally
            {
                if (q != null)
                    q.Dispose();
            }
            returnMessage = "Audit Point Written to Error Q - Successful";

            return returnMessage;
        }

        private bool IsMSMQOverThreshold(MessageQueue q, int messageThreshold)
        {
            bool returnVal = false;

            try
            {
                q.MessageReadPropertyFilter = _countFilter;
                int count = q.GetAllMessages().Count();
                if (count >= messageThreshold)
                    returnVal = true;
                else
                    returnVal = false;
            }
            catch (Exception ex)
            {
                _log.Error("Could Not Determine the Number of Messages on MSMQ", ex);
            }

            return returnVal;
        }

        private void WriteMessage(MessageQueue q, DataContractsAndProxy.MessageWrapper wrappedMessage)
        {
            bool fatal = false;
            QueueHelper<DataContractsAndProxy.MessageWrapper> queueHelper = new QueueHelper<DataContractsAndProxy.MessageWrapper>();

            string result = queueHelper.WriteMessage(q, wrappedMessage, Assembly.GetExecutingAssembly().GetType().Name, out fatal);
            if (fatal)
            {
                _log.Fatal(Assembly.GetExecutingAssembly().GetType().Name);
                _log.Fatal(result);
                throw new ApplicationException(result);
            }
        }
        #endregion

        #region Note Used
        //private void WriteMessage(MessageQueue errorQ, DataContractsAndProxy.CustomObjectToAuditAuditPoint auditPoint)
        //{
        //    string strXmlMessage = string.Empty;
        //    try
        //    {

        //        var serializer = new DataContractSerializer(auditPoint.GetType());

        //        using (var backing = new System.IO.StringWriter())
        //        {
        //            using (var writer = new System.Xml.XmlTextWriter(backing))
        //            {

        //                serializer.WriteObject(writer, auditPoint);

        //                strXmlMessage = backing.ToString();

        //                using (MessageQueueTransaction mqt = new MessageQueueTransaction())
        //                {
        //                    mqt.Begin();


        //                    //Connect to the queue
        //                    try
        //                    {
        //                        // Create a simple text message.
        //                        Message myMessage = new Message(strXmlMessage, new XmlMessageFormatter());

        //                        errorQ.Send(myMessage, mqt);
        //                        mqt.Commit();
        //                        _log.Debug(Environment.NewLine + "XML string has been submitted to MSMQ for processing by Audit Service:"
        //                      + Environment.NewLine + string.Format("{0}", strXmlMessage));
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        mqt.Abort();
        //                        _log.Fatal("Error Writing to MSMQ:  " + errorQ.QueueName, ex);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _log.Fatal(ex);
        //    }
        //}
        #endregion
    }
}
