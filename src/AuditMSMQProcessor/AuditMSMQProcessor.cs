using System;
using System.Configuration;
using System.Linq;
using System.Messaging;
using System.Reflection;
using AuditServicesCommon;
using DataContractsAndProxy;
using log4net;

namespace AuditMSMQProcessingService
{
    public class AuditMSMQProcessor
    {
        #region Properties
        MessageWrapper _message;
        static MessagePropertyFilter _countFilter = new MessagePropertyFilter();
        static ILog _log;
        #endregion

        #region Constructor
        static AuditMSMQProcessor()
        {
            //_log = log4net.LogManager.GetLogger(Constants.AUDIT_SERVICE_NAMED_LOGGER);
            _log = log4net.LogManager.GetLogger(Constants.AUDIT_SERVICE_NAMED_LOGGER);

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

        public AuditMSMQProcessor(MessageWrapper Message)
        {
            _message = Message;
        }
        #endregion

        #region Public Methods
        public object ThreadPoolCallback(Object ThreadContext)
        {
            try
            {
                _log.Info("Message Successfully Received By AuditMSMQProcessor");

                _log.Info("Attempt to Write Message to Audit DBWriter MSMQ");
                SubmitAuditMessageToDBWriterQ(_message);
                _log.Info("Attempt to Write Message to Audit DBWriter MSMQ - Successful");
            }
            catch (Exception exWriteToDBWriterQ)
            {
                _log.Fatal("Attempt to Write Message to Audit DBWriter MSMQ - UnSuccessful",exWriteToDBWriterQ);

                try
                {
                    AlternateWritePathDueToError(_message);
                }
                catch (Exception exAlternateWritePath)
                {
                    _log.Fatal(exAlternateWritePath);
                    return 0;
                }

                return 1;
            }
            return 1;
        }

        private void AlternateWritePathDueToError(MessageWrapper _message)
        {
            try
            {
                _log.Info("Alternate Process Attempt to Write Audit Message to Central Store Audit Database Direct");
                WriteMessageToDatabase(_message.WrappedCustomObjectToAuditAuditPoint);
                _log.Info("Alternate Process Audit Message Path Successful:  Written to Central Audit Database Directly");

            }
            catch (Exception exSubmitToDbDirect)
            {
                _log.Fatal("Writing Message Directly to Central Audit Database Failed, Last Ditch Effort For Recovery, Attempting to Write to Error MSMQ", exSubmitToDbDirect);
                try
                {

                    _log.Info("Alternate Process Audit Message Attempt to Path Write to Audit Error MSMQ");

                    DatabaseDownMitigation(_message);
                    _log.Info("Alternate Process Audit Message Attempt to Path Write to Audit Error MSMQ - Successful");

                }
                catch (Exception exWriteToErrorQ)
                {
                    _log.Fatal(exWriteToErrorQ);
                    _log.Info("Alternate Error Audit Message Path Not Successful:  Message was not Processed by AuditMSMQProcessor Service");

                    AllAlternateMitigationPathsFailure(_message.WrappedCustomObjectToAuditAuditPoint);
                    throw new ApplicationException("Cannot Process this Message At this time, Last Ditch Effort For Recovery failed, Multiple System Failure", exWriteToErrorQ);

                }
            }
        }

            public void AllAlternateMitigationPathsFailure(CustomObjectToAuditAuditPoint CustomObjectToAuditAuditPoint)
        {
            //clear the CustomObjectToAudit audit point from the dictionary
            //log a fatal         

            _log.Fatal("Attempts to Write Audit Point to Audit Service have been exhausted, Multiple System Failure");
            QueueHelper<CustomObjectToAuditAuditPoint> queueHelper = new QueueHelper<CustomObjectToAuditAuditPoint>();
            _log.Fatal("Message That was Not Able to Be Processed:  " + queueHelper.GetStringMessage(CustomObjectToAuditAuditPoint));

        }

             private void WriteMessageToDatabase(CustomObjectToAuditAuditPoint auditPoint)
        {
            AuditServicesDataAccess.AuditDatabase _db = new AuditServicesDataAccess.AuditDatabase();
            _db.WriteAudit(auditPoint);
        }

        private string DatabaseDownMitigation(MessageWrapper wrapper)
        {
            string returnMessage = "Audit Point Processed - Unsuccessful";

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
                _log.Fatal("Cannot Process Any Audit Messages At this time, All Mitigations Have Failed, Multiple Systems Failure has Occurred, Error Queue Name:  " + errorQueueName, exQueueInstantiation);
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
                    WriteMessage(q, wrapper);
                }
                catch (Exception ex)
                {
                    _log.Fatal("Cannot Process Any Audit Messages At this time, All Mitigations Have Failed, Multiple Systems Failure has Occurred, Error Queue Name: " + q.QueueName, ex);
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


        public string SubmitAuditMessageToDBWriterQ(MessageWrapper wrappedMessage)
        {
            string returnMessage = "Audit Point Processed - UnSuccessful";

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
                throw new ApplicationException("Cannot Receive Audit Messages due to MSMQ, Fatal");
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
                    _log.Info("Attempt To Submit Audit Message to DB Writer Q");
                    WriteMessage(q, wrappedMessage);
                    _log.Info("Attempt to Submit Audit Message to DB Writer Q - Successful");
                }
                catch (Exception ex)
                {
                    _log.Info("Attempt to Submit Audit Message to DB Writer Q - UnSuccessful");
                    _log.Fatal("Attempt to Submit Audit Message to DB Writer Q - UnSuccessful",ex);
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

            return returnMessage;

        }
        #endregion

        #region Private Helper Methods
        private void WriteMessage(MessageQueue q, DataContractsAndProxy.MessageWrapper wrappedMessage)
        {
            bool fatal = false;
            QueueHelper<MessageWrapper> queueHelper = new QueueHelper<MessageWrapper>();
            string result = queueHelper.WriteMessage(q, wrappedMessage, Assembly.GetExecutingAssembly().GetType().Name, out fatal);
            if (fatal)
            {
                _log.Fatal(Assembly.GetExecutingAssembly().GetType().Name);
                _log.Fatal(result);
                throw new ApplicationException(result);
            }
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
        #endregion

        #region Note used
        //private void WriteMessage(MessageQueue q, DataContractsAndProxy.CustomObjectToAuditAuditPoint auditPoint)
        //{
        //    string strXmlMessage = string.Empty;

        //    var serializer = new DataContractSerializer(auditPoint.GetType());

        //    using (var backing = new System.IO.StringWriter())
        //    {
        //        using (var writer = new System.Xml.XmlTextWriter(backing))
        //        {
        //            serializer.WriteObject(writer, auditPoint);

        //            strXmlMessage = backing.ToString();

        //            using (MessageQueueTransaction mqt = new MessageQueueTransaction())
        //            {
        //                mqt.Begin();

        //                //Connect to the queue
        //                try
        //                {
        //                    // Create a simple text message.
        //                    Message myMessage = new Message(strXmlMessage, new XmlMessageFormatter());

        //                    q.Send(myMessage, mqt);
        //                    mqt.Commit();
        //                    _log.Debug(Environment.NewLine + "XML string has been submitted to DB Writer MSMQ for processing by Audit MSMQ Processor Service:"
        //                  + Environment.NewLine + string.Format("{0}", strXmlMessage));
        //                }
        //                catch (Exception ex)
        //                {
        //                    mqt.Abort();
        //                    _log.Fatal("Error Writing to MSMQ:  " + q.QueueName,ex);
        //                }
        //            }
        //        }
        //    }
        //} 
        #endregion
    }
}
