using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DataContractsAndProxy
{
    [DataContract]
    public class MessageWrapper
    {
        Guid _messageIdentifier;
        CustomObjectToAuditAuditPoint _wrappedCustomObjectToAuditAuditPoint;

        [DataMember]
        public Guid MessageIdentifier
        {
            get { return _messageIdentifier; }
            set { _messageIdentifier = value; }
        }

        [DataMember]
        public CustomObjectToAuditAuditPoint WrappedCustomObjectToAuditAuditPoint
        {
            get { return _wrappedCustomObjectToAuditAuditPoint; }
            set { _wrappedCustomObjectToAuditAuditPoint = value; }
        }
    }

    [DataContract]
    public class CustomObjectToAuditAuditPoint
    {
        DateTime _auditDateTimeStamp = DateTime.MinValue;       
        string _message = "No Message Provided";
        AuditCategory _AuditCategory = AuditCategory.Unknown;
        Guid _applicationID;       
        OriginationID _originationID = OriginationID.Undefined;
        Status _reportedStatus = Status.Indeterminate;      
        Applications _applicationName = Applications.NotSpecified;
        string _referenceID;
        Dictionary<string, string> _parameters = new Dictionary<string, string>();

        /// <summary>
        /// Common ones start from 500
        /// </summary>
        [DataContract(Name = "AuditCategory")]
        public enum AuditCategory
        {
            [EnumMember]
            SignOn = 0,
            [EnumMember]
            SignOut = 1,
            [EnumMember]
            QueueCount = 2,
            [EnumMember]
            QueueList = 3,
            [EnumMember]
            QueueRemove = 4,
            [EnumMember]
            QueuePlace = 5,
            [EnumMember]
            AddNotes = 6,
            [EnumMember]
            RetrieveFromStore = 7,           
            [EnumMember]
            ValidateCustomObjectToAuditData = 101,
           
            [EnumMember]
            AccountInfoMissingException = 104,         
           
            [EnumMember]
            Unknown = 500,
            [EnumMember]
            Exception = 501
        }

        [DataContract(Name = "Status")]
        public enum Status
        {
            [EnumMember]
            Success = 0,
            [EnumMember]
            Failure = 1,
            [EnumMember]
            Indeterminate = 2
        }

        [DataContract(Name = "ApplicationName")]
        public enum Applications
        {
            [EnumMember]
            NotSpecified = 0,
            [EnumMember]
            Service1 = 1,
            [EnumMember]
            Service2 = 2,
            [EnumMember]
            SomeAutomatedJob = 3,
            [EnumMember]
            Service3 = 4,
            [EnumMember]
            Service4 = 5
        }

        [DataMember]
        public string ReferenceID
        {
            get { return _referenceID; }
            set { _referenceID = value; }
        }

        [DataMember]
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        [DataMember]
        public DateTime AuditDateTimeStamp
        {
            get { return _auditDateTimeStamp; }
            set { _auditDateTimeStamp = value; }
        }

        [DataMember]
        public Guid ApplicationID
        {
            get { return _applicationID; }
            set { _applicationID = value; }
        }

        [DataMember]
        public OriginationID OriginationID
        {
            get { return _originationID; }
            set { _originationID = value; }
        }

        [DataMember]
        public Status ReportedStatus
        {
            get { return _reportedStatus; }
            set { _reportedStatus = value; }
        }

        [DataMember]
        public AuditCategory AuditingCategory
        {
            get { return _AuditCategory; }
            set { _AuditCategory = value; }
        }
        [DataMember]
        public Applications ApplicationName
        {
            get { return _applicationName; }
            set { _applicationName = value; }
        }

        [DataMember]
        public Dictionary<string, string> Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

    }

}

