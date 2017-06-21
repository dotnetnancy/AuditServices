using System;
using System.Configuration;
using System.Linq;
using System.Messaging;
using System.Reflection;
using AuditServicesCommon;
using DotNetNancyAuditErrorMSMQProcessor;
using DataContractsAndProxy;
using log4net;

namespace DotNetNancyAuditErrorMSMQProcessingService
{
    public class SleepNotificationEventArgs : EventArgs
    {
        TimeSpan _ts;
        public SleepNotificationEventArgs(TimeSpan tsConfiguredSleepTime)
        {
            _ts = tsConfiguredSleepTime;
        }
    }

    public class DotNetNancyAuditErrorMSMQProcessor
    {
        MessageWrapper _message;
        static MessagePropertyFilter _countFilter = new MessagePropertyFilter();
        static ILog _log;
        AttemptsManagement _attemptsManager;    

        static DotNetNancyAuditErrorMSMQProcessor()
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

        public DotNetNancyAuditErrorMSMQProcessor(DataContractsAndProxy.MessageWrapper Message, AttemptsManagement attemptsManager)
        {
            _message = Message;
            _attemptsManager = attemptsManager;            
        }

        public object ThreadPoolCallback(Object ThreadContext)
        {
            try
            {
                _log.Debug("Message Successfully Received By ErrorMSMQProcessor");
                _log.Info("Attempting to Write Message to Audit DBWriter Q");
                SubmitAuditMessageToDBWriterQ(_message);
                _log.Info("Attempt to Write Message To AuditDBWriterQ - Successful");
                
            }

            catch (Exception exDBWriterQ)
            {
                _log.Info("Attempt to Write Message To AuditDBWriterQ - UnSuccessful");
                _log.Fatal("Message Not Successfully written to DBWriter MSMQ", exDBWriterQ);
                try
                {
                    _log.Info("Attempting to Write Message to DotNetNancy Central Audit Store Database Direct");

                    WriteToDatabase(_message);
                    _log.Info("Attempt to Write Message to DotNetNancy Central Audit Store Database Direct - Successful");

                }
                catch (Exception exDatabaseDirect)
                {
                    _log.Info("Attempt to Write Message to DotNetNancy Central Audit Store Database Direct - UnSuccessful");

                    try
                    {
                        _log.Info("Attempting to Write Message Back To AuditErrorQ After Attempting to Write To Audit DBWriter Q and Directly to Audit Database");

                        SubmitAuditMessageBackToErrorQ(_message);
                        _log.Info("Attempt to Write Message Back To AuditErrorQ After Attempting to Write To Audit DBWriter Q and Directly to Audit Database - Successful");

                    }
                    catch (Exception exErrorQ)
                    {
                        _log.Info("Attempt to Write Message Back To AuditErrorQ After Attempting to Write To Audit DBWriter Q and Directly to Audit Database - UnSuccessful");
                        _log.Fatal("Tried to Process a Message and put it back on the AuditErrorQ, Multiple System Failure, Cannot do Anything with Message", exErrorQ);
                        try
                        {
                            _log.Info("Attempt to write Audit Payload Message to Log File - Multiple System Failure - Cannot Write Audit Point Message to the Database or the Error MSMQ");
                            AllAlternateMitigationPathsFailure(_message.WrappedCustomObjectToAuditAuditPoint);
                            _log.Info("Attempt to write Audit Payload Message to Log File - Multiple System Failure - Log File Written - Successful - this log is the only place that this Audit Message Can now Be Found");

                        }
                        catch (Exception ex)
                        {
                            //nothing else can be done here
                            _log.Info("Attempt to write Audit Payload Message to Log File - Multiple System Failure - Log File Written - Unsuccessful - Audit Message Payload Lost", ex);
                            _log.Fatal("Attempt to write Audit Payload Message to Log File - Multiple System Failure - Log File Written - Unsuccessful - Audit Message Payload Lost", ex);


                        }
                    }
                }
                return 0;
            }
            return 1;
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

        private void WriteToDatabase(MessageWrapper message)
        {
            try
            {
                _log.Info("Attempting to Write Message To Audit Central Store Database");

                WriteMessageToDatabase(message.WrappedCustomObjectToAuditAuditPoint);

                _log.Info("Attempt to Write Message to Audit Central Store Database - Successful");
            }
            catch (Exception exDbDown)
            {
                _log.Fatal(exDbDown);
                _log.Info("Attempt to Write Message to Audit Central Store Database - UnSuccessful");
                throw new ApplicationException("Attempt to Write Message to Audit Central Store Database - UnSuccessful", exDbDown);
            }
        }


        public string SubmitAuditMessageBackToErrorQ(DataContractsAndProxy.MessageWrapper wrappedMessage)
        {
            string returnMessage = "Audit Point Received - Successful";
            
            string queuePath = ConfigurationManager.AppSettings["errorQueueName"];
            string messageThresholdString = ConfigurationManager.AppSettings["messageQueueThreshold"];
            int messageThreshold = Convert.ToInt32((messageThresholdString == null) ? "1000" : messageThresholdString);

            MessageQueue q = null;

            try
            {
                q = new MessageQueue(queuePath);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Cannot Receive Audit Messages due to MSMQ " + queuePath);
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
                    _log.Fatal("Error MSMQ Message Threshold Exceeded, Please Check MSMQ " + q.QueueName);
                }
                try
                {
                    WriteMessage(q, wrappedMessage);                        
                }
                catch (Exception ex)
                {
                    _log.Fatal(ex);
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


        public string SubmitAuditMessageToDBWriterQ(DataContractsAndProxy.MessageWrapper wrappedMessage)
        {
            string returnMessage = "Audit Point Received - Successful";

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
                    int numberOfTries = _attemptsManager.SubmitAttempt(wrappedMessage);                    
                    WriteMessage(q, wrappedMessage);                   
                    
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

            return returnMessage;

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

        private bool IsMSMQOverThreshold(MessageQueue q, int messageThreshold)
        {
            bool returnVal = false;

            try
            {
                q.MessageReadPropertyFilter = _countFilter;
                int count = q.GetAllMessages().Count();
                if (count >= messageThreshold)
                {
                    returnVal = true;
                }
                else
                {

                    returnVal = false;
                }
            }
            catch (Exception ex)
            {
                _log.Error("Could Not Determine the Number of Messages on MSMQ", ex);
            }

            return returnVal;
        }

    }
}
