using System;
using System.Configuration;
using System.Linq;
using System.Messaging;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using Amib.Threading;
using DotNetNancyAuditErrorMSMQProcessor;
using DataContractsAndProxy;
using log4net;
using AuditServicesCommon;

namespace DotNetNancyAuditErrorMSMQProcessingService
{
    public class DotNetNancyAuditErrorMSMQImplementation
    {
        #region Properties
        SmartThreadPool _tp = new SmartThreadPool();
        MessagePropertyFilter _countFilter = new MessagePropertyFilter();
        System.Timers.Timer _keepTabsOnQueueSizeCounter;
        static int _queueSizeCheckMilliseconds;
        static int _minThreads;
        static int _maxThreads;
        string _queueName;

        public string QueueName
        {
            get { return _queueName; }
            set { _queueName = value; }
        }

        static int _messageThreshold;
        static ILog _log;
        static AttemptsManagement _attemptsManager;
        static int _configuredNumberOfAttempts;
        static int _configuredSleepValueInMinutes;
        #endregion

        #region Constructor
        static DotNetNancyAuditErrorMSMQImplementation()
        {
            //_log = log4net.LogManager.GetLogger(Constants.AUDIT_SERVICE_NAMED_LOGGER);
            _log = log4net.LogManager.GetLogger(Constants.AUDIT_SERVICE_NAMED_LOGGER);

            //defaults for configurable items
            _queueSizeCheckMilliseconds = 300000;
            _minThreads = 5;
            _maxThreads = 10;
            //q name is required in config
            _messageThreshold = 1000;
            _configuredNumberOfAttempts = 5;
            _configuredSleepValueInMinutes = 10;
        }
        #endregion


        public void Start()
        {
            _queueName = null;

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

                string configuredNumberOfAttempts = ConfigurationManager.AppSettings["ConfiguredNumberOfAttempts"];
                string configuredSleepValueInMinutes = ConfigurationManager.AppSettings["ConfiguredSleepValueInMinutes"];

                int attempts = configuredNumberOfAttempts == null ? _configuredNumberOfAttempts : Convert.ToInt32(configuredNumberOfAttempts);
                int sleep = configuredSleepValueInMinutes == null ? _configuredSleepValueInMinutes : Convert.ToInt32(configuredSleepValueInMinutes);

                string minThreads = ConfigurationManager.AppSettings["MinThreads"];
                string maxThreads = ConfigurationManager.AppSettings["MaxThreads"];

                _attemptsManager = new AttemptsManagement(attempts, sleep, this);

                //don't forget to start the threadpool...
                _tp.MinThreads = minThreads == null ? _minThreads : Convert.ToInt32(minThreads);
                _tp.MaxThreads = maxThreads == null ? _maxThreads : Convert.ToInt32(maxThreads);

                _tp.Start();

                string messageQueueSetting = ConfigurationManager.AppSettings["errorQueueName"];
                _queueName = messageQueueSetting == null ? null : messageQueueSetting;

                if (_queueName == null)
                {
                    throw new ApplicationException("No valid error queueName setting in the Configuration File");
                }

                // Create a transaction queue using System.Messaging API

                if (!MessageQueue.Exists(_queueName))
                    MessageQueue.Create(_queueName, true);

                //Connect to the queue
                MessageQueue Queue = new MessageQueue(_queueName);

                System.Messaging.MessageQueue.EnableConnectionCache = false;
                watchQueueDelegate dlgt = new watchQueueDelegate(watchQueue);
                IAsyncResult ar = dlgt.BeginInvoke(_queueName, new AsyncCallback(watchQueueCallback), dlgt);

                _log.Debug("ErrorMSMQProcessingService is running");

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
                {
                    q.Dispose();
                }
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
                        {
                            _log.Fatal("MSMQ Message Threshold Setup in Configuration Reached or Exceeded, This indicates that the Message Q On This Machine is Not Processing Messages");
                        }
                        else
                        {
                            _log.Debug("Queue Size Not Exceeded in this Interval's Check " + Assembly.GetExecutingAssembly().GetType().Name);
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


        public void unwatchQueue(string queuePath)
        {
            MessageQueue q = new MessageQueue(queuePath);
            q.ReceiveCompleted -= new ReceiveCompletedEventHandler(q_ReceiveCompleted);

        }

        public void watchQueue(string queuePath)
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
            //Thread.Sleep(new TimeSpan(0,_configuredSleepValueInMinutes,0));
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

                bool exists = false;

                int numberOfAttempts = _attemptsManager.Read(wrappedMessage, out exists);

                //don't queue it anymore and remove it from the attempts processing
                if (_attemptsManager.AttemptsExceeded(numberOfAttempts))
                {
                    _attemptsManager.AttemptsExceededMitigation(wrappedMessage);
                    Thread.Sleep(new TimeSpan(0, _attemptsManager.ConfiguredSleepValueInMinutes, 0));
                }
                else
                {
                    //keep trying until the number of attempts is exceeded
                    DotNetNancyAuditErrorMSMQProcessor processor = new DotNetNancyAuditErrorMSMQProcessor(wrappedMessage, _attemptsManager);
                    _tp.QueueWorkItem(new Amib.Threading.WorkItemCallback(processor.ThreadPoolCallback));
                }

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


    }

}