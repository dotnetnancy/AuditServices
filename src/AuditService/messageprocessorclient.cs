using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.MsmqIntegration;
using WCFMQService;

namespace WCFMQService
{
    /// <summary>
    /// The messageprocessorclient serves the function of a proxy helper class and provides a handle to the 
    /// queue via the binding information found in the 'App.config' file in the Client project
    /// </summary>
    #region ServiceContract
    [System.ServiceModel.ServiceContractAttribute(Namespace = "http://WCFMQService")]
    public interface IMessageProcessor
    {
        [OperationContract(IsOneWay = true, Action = "*")]
        void SubmitStringMessage(MsmqMessage<string> msg);       
    }
    #endregion

    #region OperationContract
    public partial class MessageProcessorClient : System.ServiceModel.ClientBase<IMessageProcessor>, IMessageProcessor
    {

        public MessageProcessorClient()
        {}

        public MessageProcessorClient(string configurationName)
            : base(configurationName)
        {}

        public MessageProcessorClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress address)
            : base(binding, address)
        {}

        public void SubmitStringMessage(MsmqMessage<string> msg)
        {
            base.Channel.SubmitStringMessage(msg);
        }
      
    }
    #endregion
}
