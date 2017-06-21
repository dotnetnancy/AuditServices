using System;
using System.Configuration;
using System.Linq;
using System.Messaging;
using System.Reflection;
using System.ServiceModel;
using AuditServicesCommon;
using log4net;
using DataContractsAndProxy;

namespace AuditingService
{
    [ServiceBehavior(Namespace = "http://DotNetNancy.com/services/AuditService",
   InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class AuditService : IAuditService
    {
        #region Properties
        MessagePropertyFilter _countFilter = new MessagePropertyFilter();
        string _queuePath;
        string _messageThresholdString;
        int _messageThreshold;
        static ILog _log;
        #endregion

        #region Constructor
        static AuditService()
        {
            _log = log4net.LogManager.GetLogger(Constants.AUDIT_SERVICE_NAMED_LOGGER);
        }

        public AuditService()
        {
            _log = log4net.LogManager.GetLogger(Constants.AUDIT_SERVICE_NAMED_LOGGER);
            try
            {
                if (!MessageQueue.Exists(ConfigurationManager.AppSettings["queueName"]))
                    MessageQueue.Create(ConfigurationManager.AppSettings["queueName"], true);

                _countFilter.AdministrationQueue = false;
                _countFilter.ArrivedTime = false;
                _countFilter.CorrelationId = false;
                _countFilter.Priority = false;
                _countFilter.ResponseQueue = false;
                _countFilter.SentTime = false;
                _countFilter.Body = false;
                _countFilter.Label = false;
                _countFilter.Id = false;

                _queuePath = ConfigurationManager.AppSettings["queueName"];
                _messageThresholdString = ConfigurationManager.AppSettings["messageQueueThreshold"];
                _messageThreshold = Convert.ToInt32((_messageThresholdString == null) ? "1000" : _messageThresholdString);
                _log.Debug("AuditService constructor called");
            }
            catch (Exception ex)
            {
                _log.Fatal(ex);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Submits the audit to auditmessagequeue
        /// </summary>
        /// <param name="auditPoint">The audit point.</param>
        /// <returns>Status of Queueing</returns>
        public string SubmitAudit(CustomObjectToAuditAuditPoint auditPoint)
        {
            string returnMessage = "Audit Point Received - Unsuccessful";
            string queuePath = ConfigurationManager.AppSettings["queueName"];
            string messageThresholdString = ConfigurationManager.AppSettings["messageQueueThreshold"];
            int messageThreshold = Convert.ToInt32((messageThresholdString == null) ? "1000" : messageThresholdString);
            MessageQueue q = null;
            MessageWrapper wrapper = null;
            try
            {
                q = new MessageQueue(queuePath);
            }
            catch (Exception exQueueInstantiation)
            {
                _log.Fatal("Cannot Receive Audit Messages due to MSMQ " + q.QueueName, exQueueInstantiation);
                try
                {
                    AlternateWritePathDueToError(auditPoint, out wrapper);
                }
                catch(Exception exAlternatePathError)
                {
                    _log.Fatal("Cannot Receive Audit Messages and Alternate Path Mitigations also Failed, Major multiple Systems Outage", exAlternatePathError);
                    throw new ApplicationException("Cannot Receive Audit Messages and Alternate Path Mitigations also Failed, Major multiple Systems Outage", exAlternatePathError);
                }
            }
            finally
            {
                if (q != null)
                {
                    q.Dispose();
                }
            }
            try
            {
                if (IsMSMQOverThreshold(q, messageThreshold))
                {
                    _log.Fatal("Audit MSMQ Message Threshold Exceeded, Please Check MSMQ, still accepting Messages But Unfixed Could use up Maximum Memory on Machine" + q.QueueName);
                }
                try
                {
                    WriteMessage(q, auditPoint, out wrapper);
                }
                catch (Exception exWriteToReceiverQ)
                {
                    _log.Fatal("Cannot Receive Audit Messages due to MSMQ " + q.QueueName, exWriteToReceiverQ);
                    try
                    {
                        AlternateWritePathDueToError(auditPoint, out wrapper);
                    }
                    catch (Exception exAlternatePathError)
                    {
                        _log.Fatal("Cannot Receive Audit Messages and Alternate Path Mitigations also Failed, Major multiple Systems Outage", exAlternatePathError);
                        throw new ApplicationException("Cannot Receive Audit Messages and Alternate Path Mitigations also Failed, Major multiple Systems Outage", exAlternatePathError);
                    }
                }
            }
            finally
            {
                if (q != null)
                {
                    q.Dispose();
                }
            }
            returnMessage = "Audit Point Received - Successful";
            return returnMessage;
        }
        #endregion

        #region Private Helpers
        /// <summary>
        /// This is called when we have a failure to put on to the receiver Q
        /// </summary>
        private void AlternateWritePathDueToError(CustomObjectToAuditAuditPoint CustomObjectToAuditAuditPoint, out MessageWrapper wrapper)
        {
            _log.Info("Alternate Receive Audit Message Path Initiated - Cause:  AuditService Receiver MSMQ Error");

            wrapper = null;

            try
            {
                SubmitAuditMessageToDBWriterQ(CustomObjectToAuditAuditPoint, out wrapper);
                _log.Info("Alternate Receive Audit Message Path Successful:  Written to Audit DB Writer MSMQ");
            }
            catch (Exception exSubmitToDBWriterQ)
            {
                _log.Fatal("Cannot Receive Audit Messages due to MSMQ, Fatal, Mitigations Being Attempted", exSubmitToDBWriterQ);

                try
                {
                    WriteMessageToDatabase(CustomObjectToAuditAuditPoint);
                    _log.Info("Alternate Receive Audit Message Path Successful:  Written to Central Audit Database Directly");

                }
                catch (Exception exSubmitToDbDirect)
                {
                    _log.Fatal("Writing Message Directly to Central Audit Database Failed, Last Ditch Effort For Recovery, Attempting to Write to Error MSMQ", exSubmitToDbDirect);
                    try
                    {
                        DatabaseDownMitigation(CustomObjectToAuditAuditPoint);
                        _log.Info("Alternate Receive Audit Message Path Successful:  Written to Audit Error MSMQ");

                    }
                    catch (Exception exWriteToErrorQ)
                    {
                        _log.Fatal(exWriteToErrorQ);
                        _log.Info("Alternate Receive Audit Message Path Not Successful:  Message was not Received by Audit Service");

                        AllAlternateMitigationPathsFailure(CustomObjectToAuditAuditPoint);
                        throw new ApplicationException("Cannot Accept Your Message At this time, Last Ditch Effort For Recovery failed, Multiple System Failure", exWriteToErrorQ);

                    }
                }
            }
            
        }

        public string SubmitAuditMessageToDBWriterQ(CustomObjectToAuditAuditPoint CustomObjectToAuditAuditPoint, out MessageWrapper wrapper)
        {
            
            string returnMessage = "Audit Point Received - Unsuccessful";

            string queuePath = ConfigurationManager.AppSettings["dbWriterQueueName"];
            string messageThresholdString = ConfigurationManager.AppSettings["messageQueueThreshold"];
            int messageThreshold = Convert.ToInt32((messageThresholdString == null) ? "1000" : messageThresholdString);

            MessageQueue q = null;

            try
            {
                q = new MessageQueue(queuePath);
            }
            catch (Exception ex)
            {
                _log.Fatal("Cannot Receive Audit Messages due to MSMQ, Fatal, Mitigations Being Attempted, QueuePath:  " + queuePath, ex);
                throw new ApplicationException("Cannot Receive Audit Messages due to MSMQ, Fatal, Mitigations Being Attempted, QueuePath:  " + queuePath,ex);
            }
            finally
            {
                if (q != null)
                {
                    q.Dispose();
                }
            }

            try
            {
                if (IsMSMQOverThreshold(q, messageThreshold))
                {
                    _log.Fatal("Audit MSMQ Message Threshold Exceeded, Please Check MSMQ " + q.QueueName);
                }
                try
                {
                    WriteMessage(q, CustomObjectToAuditAuditPoint, out wrapper);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Cannot Write Audit Messages to MSMQ: " + q.QueueName);
                }
            }
            finally
            {
                if (q != null)
                {
                    q.Dispose();
                }
            }

            returnMessage = "Audit Point Received - Successful";
            return returnMessage;

        }

        private void WriteMessageToDatabase(CustomObjectToAuditAuditPoint auditPoint)
        {
            AuditServicesDataAccess.AuditDatabase _db = new AuditServicesDataAccess.AuditDatabase();
            _db.WriteAudit(auditPoint);
        }

        private string DatabaseDownMitigation(CustomObjectToAuditAuditPoint auditPoint)
        {
            MessageWrapper wrapper = null;
            string returnMessage = "Audit Point Received - Unsuccessfully";

            string messageThresholdString = ConfigurationManager.AppSettings["messageQueueThreshold"];
            int messageThreshold = Convert.ToInt32((messageThresholdString == null) ? "1000" : messageThresholdString);

            string messageQueueSetting = ConfigurationManager.AppSettings["errorQueueName"];
            string errorQueueName = messageQueueSetting == null ? "no message queue defined in config" : messageQueueSetting;

            //this is our last ditch effort to keep this audit , we put it on an error
            //Q which then should be processed when the database is back up.            string queuePath = ConfigurationManager.AppSettings["dbWriterQueueName"];

            MessageQueue q = null;

            try
            {

                if (!MessageQueue.Exists(errorQueueName))
                    MessageQueue.Create(errorQueueName, true);

                q = new MessageQueue(errorQueueName);
            }
            catch (Exception exQueueInstantiation)
            {
                _log.Fatal("Cannot Accept Any Audit Messages At this time, All Mitigations Have Failed, Multiple Systems Failure has Occurred, Error Queue Name:  " + errorQueueName, exQueueInstantiation);
                throw new ApplicationException("Cannot instantiate or create MSMQ " + errorQueueName, exQueueInstantiation);
            }
            finally
            {
                if (q != null)
                    q.Dispose();
            }

            try
            {
                if (IsMSMQOverThreshold(q, messageThreshold))
                {
                    _log.Fatal("Audit MSMQ Message Threshold Exceeded, Please Check MSMQ, still accepting Messages But Unfixed Could use up Maximum Memory on Machine" + q.QueueName);
                }

                try
                {
                    WriteMessage(q, auditPoint, out wrapper);
                }
                catch (Exception ex)
                {
                    _log.Fatal("Cannot Accept Any Audit Messages At this time, All Mitigations Have Failed, Multiple Systems Failure has Occurred, Error Queue Name: " + q.QueueName, ex);
                    throw new ApplicationException("Cannot Accept Any Audit Messages At this time, All Mitigations Have Failed, Multiple Systems Failure has Occurred, Error Queue Name: " + q.QueueName, ex);
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

        public void AllAlternateMitigationPathsFailure(CustomObjectToAuditAuditPoint CustomObjectToAuditAuditPoint)
        {
            //clear the CustomObjectToAudit audit point from the dictionary
            //log a fatal         

            _log.Fatal("Attempts to Write Audit Point to Audit Service have been exhausted, Multiple System Failure");
            QueueHelper<CustomObjectToAuditAuditPoint> queueHelper = new QueueHelper<CustomObjectToAuditAuditPoint>();
            _log.Fatal("Message That was Not Able to Be Processed:  " + queueHelper.GetStringMessage(CustomObjectToAuditAuditPoint));

        }
        /// <summary>
        /// Writes the message to Q.
        /// </summary>
        /// <param name="q">The Msg Q</param>
        /// <param name="auditPoint">The audit point.</param>
        private void WriteMessage(MessageQueue q, DataContractsAndProxy.CustomObjectToAuditAuditPoint auditPoint, out MessageWrapper wrapper)
        {
            wrapper = new MessageWrapper();
            wrapper.MessageIdentifier = Guid.NewGuid();
            wrapper.WrappedCustomObjectToAuditAuditPoint = auditPoint;
            bool fatal = false;
            QueueHelper<DataContractsAndProxy.MessageWrapper> queueHelper = new QueueHelper<DataContractsAndProxy.MessageWrapper>();
            string result = queueHelper.WriteMessage(q, wrapper, Assembly.GetExecutingAssembly().GetType().Name, out fatal);
            if (fatal)
            {
                _log.Fatal(Assembly.GetExecutingAssembly().GetType().Name);
                _log.Fatal(result);
                throw new ApplicationException(result);
            }
        }

        /// <summary>
        /// Determines whether MSMQ exceeds the [configured] threshold.
        /// </summary>
        /// <param name="q">The Msg Q</param>
        /// <param name="messageThreshold">The message threshold.</param>
        /// <returns>
        /// 	<c>true</c> if MSMQ exceeds threshold; otherwise, <c>false</c>.
        /// </returns>
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
                _log.Error("Could Not Determine the Number of Messages on the MSMQ", ex);
            }

            return returnVal;
        }
        #endregion

        #region Not used
        //private void WriteMessage(MessageQueue q, DataContractsAndProxy.CustomObjectToAuditAuditPoint auditPoint)
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

        //                        q.Send(myMessage, mqt);
        //                        mqt.Commit();
        //                        _log.Debug(Environment.NewLine + "XML string has been submitted to MSMQ for processing by Audit Service:"
        //                      + Environment.NewLine + string.Format("{0}", strXmlMessage));
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        mqt.Abort();
        //                        _log.Fatal("Error Writing to MSMQ:  " + q.QueueName, ex);
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
