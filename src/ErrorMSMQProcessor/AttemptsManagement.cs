using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using AuditServicesCommon;
using DataContractsAndProxy;
using log4net;
using AuditServicesDataAccess;

namespace DotNetNancyAuditErrorMSMQProcessor
{

    public class AttemptsManagement
    {
        #region Properties
        Dictionary<Guid, int> _attemptsDictionary = new Dictionary<Guid, int>();
        //defaults for configured values if not provided
        int _configuredNumberOfAttempts = 5;
        int _configuredSleepValueInMinutes = 10;
        DotNetNancyAuditErrorMSMQProcessingService.DotNetNancyAuditErrorMSMQImplementation _implementation;
        private ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();
        static ILog _log;
        #endregion

        public DotNetNancyAuditErrorMSMQProcessingService.DotNetNancyAuditErrorMSMQImplementation Implementation
        {
            get { return _implementation; }
            set { _implementation = value; }
        }

        public int ConfiguredSleepValueInMinutes
        {
            get
            {
                try
                {
                    _readerWriterLock.EnterReadLock();

                    return _configuredSleepValueInMinutes;
                }
                finally
                {
                    _readerWriterLock.ExitReadLock();
                }
            }
        }

        public AttemptsManagement(int configuredNumberOfAttempts, int configuredSleepValueInMinutes,
            DotNetNancyAuditErrorMSMQProcessingService.DotNetNancyAuditErrorMSMQImplementation implementation)
        {

            if (configuredNumberOfAttempts > 0)
            {
                _configuredNumberOfAttempts = configuredNumberOfAttempts;
            }

            if (configuredSleepValueInMinutes > 0)
            {
                _configuredSleepValueInMinutes = configuredSleepValueInMinutes;
            }

            _implementation = implementation;
            _log = log4net.LogManager.GetLogger(Constants.AUDIT_SERVICE_NAMED_LOGGER);
        }

        public int Read(MessageWrapper key, out bool exists)
        {
            exists = false;
            _readerWriterLock.EnterReadLock();
            try
            {
                if (_attemptsDictionary.ContainsKey(key.MessageIdentifier))
                {
                    exists = true;
                    return _attemptsDictionary[key.MessageIdentifier];
                }
                else
                {
                    return -1;
                }
            }
            finally
            {
                _readerWriterLock.ExitReadLock();
            }
        }



        public int SubmitAttempt(MessageWrapper wrappedMessage)
        {
            int numberOfTries = 0;

            //change value of a collection member           
            try
            {
                bool exists = false;
                numberOfTries = Read(wrappedMessage, out exists);

                try
                {
                    _readerWriterLock.EnterWriteLock();

                    if (exists)
                        _attemptsDictionary[wrappedMessage.MessageIdentifier] += 1;
                    else
                        _attemptsDictionary.Add(wrappedMessage.MessageIdentifier, 1);
                }
                finally
                {
                    _readerWriterLock.ExitWriteLock();
                }

            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
            return numberOfTries;

        }

        public bool AttemptsExceeded(int numberOfTries)
        {
            bool attemptsExceeded = false;

            if (numberOfTries > _configuredNumberOfAttempts)
            {
                attemptsExceeded = true;
            }

            return attemptsExceeded;
        }

        public void AttemptsExceededMitigation(MessageWrapper wrappedMessage)
        {
            //clear the CustomObjectToAudit audit point from the dictionary
            //log a fatal

            Remove(wrappedMessage);

            try
            {
                _log.Fatal("Attempts to Write Audit Point to AuditDBWriterMSMQ have been exceeded, number of tries:  " 
                    + _configuredNumberOfAttempts.ToString());
                _log.Info("Attempting to Write to the Central Audit Database Directly, this is a last Ditch Effort for recovery");
                AuditDatabase auditDataAccess = new AuditDatabase();
                auditDataAccess.WriteAudit(wrappedMessage.WrappedCustomObjectToAuditAuditPoint);
                _log.Info("Attempt to Write Directly to the Central Store Audit Database was Successful, There are still issues with the AuditDBWriterMSMQ that need to be resolved");
            }
               
            catch (Exception)
            {

                _log.Fatal("Attempt to Write Directly to the Central Store Audit Database Directly was UnSuccessful");
                _log.Fatal("Attempts to Write Audit Point to Audit database have been exceeded, number of tries:  "
                    + _configuredNumberOfAttempts.ToString());
                QueueHelper<MessageWrapper> queueHelper = new QueueHelper<MessageWrapper>();
                _log.Fatal(queueHelper.GetStringMessage(wrappedMessage));

            }

        }

        private void Remove(MessageWrapper wrappedMessage)
        {
            try
            {
                _readerWriterLock.EnterWriteLock();
                _attemptsDictionary.Remove(wrappedMessage.MessageIdentifier);
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }
        }
    }
}
