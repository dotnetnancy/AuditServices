using System;
using System.Configuration;
using System.Linq;
using System.Messaging;
using System.Reflection;
using System.Runtime.Serialization;
using Amib.Threading;
using DataContractsAndProxy;
using log4net;
using AuditServicesCommon;

namespace AuditMSMQProcessingService
{
    public class AuditMSMQImplementation
    {
        #region Properties
        SmartThreadPool _tp = new SmartThreadPool();
        MessagePropertyFilter _countFilter = new MessagePropertyFilter();
        System.Timers.Timer _keepTabsOnQueueSizeCounter;
        static int _queueSizeCheckMilliseconds;
        static int _minThreads;
        static int _maxThreads;
        static string _queueName;
        static int _messageThreshold;
        static ILog _log;
        #endregion


        #region Constructor
        static AuditMSMQImplementation()
        {
            //_log = log4net.LogManager.GetLogger(Constants.AUDIT_SERVICE_NAMED_LOGGER);
            _log = log4net.LogManager.GetLogger(Constants.AUDIT_SERVICE_NAMED_LOGGER);

            //defaults for configurable items
            _queueSizeCheckMilliseconds = 300000;
            _minThreads = 5;
            _maxThreads = 10;
            //q name is required in config
            _queueName = null;
            _messageThreshold = 1000;
        }
        #endregion

        public void Start()
        {
            try
            {
                string queueSizeCheckMilliseconds = ConfigurationManager.AppSettings["QueueSizeCheckIntervalInMilliseconds"];
                _keepTabsOnQueueSizeCounter =
                    new System.Timers.Timer((queueSizeCheckMilliseconds == null) ? _queueSizeCheckMilliseconds : Convert.ToInt32(queueSizeCheckMilliseconds));
                _keepTabsOnQueueSizeCounter.Start();

                //you’ll want to use a MessageReadPropertyFilter in order to basically filter out all the data to keep the foot print small.
                _countFilter.AdministrationQueue = false;
                _countFilter.ArrivedTime = false;
                _countFilter.CorrelationId = false;
                _countFilter.Priority = false;
                _countFilter.ResponseQueue = false;
                _countFilter.SentTime = false;
                _countFilter.Body = false;
                _countFilter.Label = false;
                _countFilter.Id = false;

                _keepTabsOnQueueSizeCounter.Elapsed += new System.Timers.ElapsedEventHandler(_keepTabsOnQueueSizeCounter_Elapsed);

                string minThreads = ConfigurationManager.AppSettings["MinThreads"];
                string maxThreads = ConfigurationManager.AppSettings["MaxThreads"];

                //don't forget to start the threadpool...
                _tp.MinThreads = minThreads == null ? _minThreads : Convert.ToInt32(minThreads);
                _tp.MaxThreads = maxThreads == null ? _maxThreads : Convert.ToInt32(maxThreads);

                _tp.Start();

                string messageQueueSetting = ConfigurationManager.AppSettings["queueName"];
                _queueName = messageQueueSetting == null ? null : messageQueueSetting;

                if (_queueName == null)
                {
                    throw new ApplicationException("No valid queueName setting in the Configuration File");
                }

                // Create a transaction queue using System.Messaging API
                if (!MessageQueue.Exists(_queueName))
                    MessageQueue.Create(_queueName, true);

                //Connect to the queue
                MessageQueue Queue = new MessageQueue(_queueName);

                System.Messaging.MessageQueue.EnableConnectionCache = false;
                watchQueueDelegate dlgt = new watchQueueDelegate(watchQueue);
                IAsyncResult ar = dlgt.BeginInvoke(_queueName, new AsyncCallback(watchQueueCallback), dlgt);
                _log.Debug("AuditMSMQProcessingService is running");
            }
            catch (Exception ex)
            {
                _log.Fatal(ex);
            }
        }

        void _keepTabsOnQueueSizeCounter_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _log.Debug("in check q size timer " + Assembly.GetExecutingAssembly().GetType().Name);
            _keepTabsOnQueueSizeCounter.Enabled = false;
            bool error = false;
            MessageQueue q = null;
            string queuePath = _queueName;

            try
            {
                q = new MessageQueue(queuePath);
            }
            catch (Exception ex)
            {
                error = true;
                _log.Fatal(ex);
            }
            finally
            {
                if (q != null)
                    q.Dispose();
            }

            if (!error)
            {
                try
                {
                    string messageThresholdString = ConfigurationManager.AppSettings["messageQueueThreshold"];
                    int messageThreshold = messageThresholdString == null ? _messageThreshold : Convert.ToInt32(messageThresholdString);
                    q = new MessageQueue(queuePath);

                    try
                    {
                        q.MessageReadPropertyFilter = _countFilter;
                        int count = q.GetAllMessages().Count();
                        if (count >= messageThreshold)
                            _log.Fatal("MSMQ Message Threshold Setup in Configuration Reached or Exceeded, This indicates that the Message Q On This Machine is Not Processing Messages");
                        else
                            _log.Debug("Queue Size Not Exceeded in this Interval's Check " + Assembly.GetExecutingAssembly().GetType().Name);
                    }
                    finally
                    {
                        if (q != null)
                        {
                            q.Dispose();
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
            }

            _keepTabsOnQueueSizeCounter.Enabled = true;

        }

        void watchQueueCallback(IAsyncResult ar)
        {
            // Retrieve the delegate.
            watchQueueDelegate dlgt = (watchQueueDelegate)ar.AsyncState;
            // Call EndInvoke to retrieve the results.
            dlgt.EndInvoke(ar);
        }

        delegate void watchQueueDelegate(string queuePath);

        delegate void auditToProcessDelegate(DataContractsAndProxy.MessageWrapper wrappedMessage,
           ReceiveCompletedEventArgs asyncResult);

        #region Private Helpers
        private void watchQueue(string queuePath)
        {
            while (true)
            {
                try
                {
                    MessageQueue q = new MessageQueue(queuePath);

                    q.Formatter = new BinaryMessageFormatter();
                    //q.Formatter = new XmlMessageFormatter(new Type[] { typeof(DataContractsAndProxy.CustomObjectToAuditAuditPoint) });
                    q.ReceiveCompleted += new ReceiveCompletedEventHandler(q_ReceiveCompleted);
                    q.BeginReceive(new TimeSpan(0, 0, 15));
                    break;
                }
                catch
                {
                    //do nothing simply repeat the loop
                }
            }
        }

        private void q_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            //Thread.Sleep(70000);
            MessageQueue q = (MessageQueue)sender;
            DataContractsAndProxy.MessageWrapper wrappedMessage = null;

            try
            {
                q.Formatter = new BinaryMessageFormatter();
                //q.Formatter = new XmlMessageFormatter(new Type[] { typeof(String) });
                // Pause the asynchronous receive operation while processing current message.
                System.Messaging.Message msg = q.EndReceive(e.AsyncResult);

                DataContractSerializer serializer = new DataContractSerializer(typeof(MessageWrapper));
                wrappedMessage = (MessageWrapper)serializer.ReadObject(msg.BodyStream);

                AuditMSMQProcessor processor = new AuditMSMQProcessor(wrappedMessage);
                _tp.QueueWorkItem(new Amib.Threading.WorkItemCallback(processor.ThreadPoolCallback));

                q.BeginReceive(new TimeSpan(0, 0, 15));
            }
            catch (System.Messaging.MessageQueueException ex)
            {
                if (ex.MessageQueueErrorCode == System.Messaging.MessageQueueErrorCode.IOTimeout)
                {
                    q.BeginReceive(new TimeSpan(0, 0, 15));
                }
                else
                {
                    watchQueue("FormatName:" + q.FormatName);
                    q.Dispose();
                }
            }
            catch (Exception ex)
            {
                _log.Debug("Error Receiving Message off of MSMQ", ex);
                _log.Fatal("This may indicate that a client is putting messages on the MSMQ that the" + Assembly.GetExecutingAssembly().GetType().Name + " cannot Handle", ex);
                watchQueue("FormatName:" + q.FormatName);
                q.Dispose();
            }
        }
        #endregion

    }

}