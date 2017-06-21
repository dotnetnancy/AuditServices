using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Messaging;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;

namespace AuditServicesCommon
{
    public class QueueHelper<T>
    {

        public string WriteMessage(MessageQueue q, T instance, string serviceName, out bool fatal)
        {
            string result = "Write to MSMQ Failed " + string.Format("{0}",q.QueueName);
            string strXmlMessage = "Could Not Determine the Message Contents";
            fatal = false;

            try
            {
                DataContractSerializer dcs = new DataContractSerializer(typeof(T));
                using (MemoryStream ms = new MemoryStream())
                {
                    dcs.WriteObject(ms, instance);
                  

                    using (MessageQueueTransaction mqt = new MessageQueueTransaction())
                    {
                        mqt.Begin();

                        //Connect to the queue
                        try
                        {
                            // Create a simple text message.
                            Message myMessage = new Message();
                            ms.Position = 0;
                            myMessage.BodyStream = ms;

                            q.Send(myMessage, mqt);
                            mqt.Commit();
                           
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                mqt.Abort();
                            }
                            catch (Exception ex1)
                            {
                                fatal = true;
                                result = "Error Writing to MSMQ:  " + q.QueueName + ex1.StackTrace;
                            }
                            fatal = true;
                            result = "Error Writing to MSMQ:  " + q.QueueName + ex.StackTrace;                          
                        }
                       
                    }
                }

            }
            catch (Exception ex)
            {
                fatal = true;
                result = ex.StackTrace;
            }

            if (!fatal)
            {
                try
                {
                    DataContractSerializer dcs = new DataContractSerializer(typeof(T));
                    using (MemoryStream ms = new MemoryStream())
                    {
                        dcs.WriteObject(ms, instance);

                        strXmlMessage = GetStringMessage(ms, dcs, instance);
                        result = (Environment.NewLine + "XML string has been submitted to MSMQ for processing by " + serviceName + ":"
                      + Environment.NewLine + string.Format("{0}", strXmlMessage));
                    }
                }
                catch (Exception)
                {
                    //do nothing right now this is just for logging don't stop regular processing over it
                }
            }
            return result;
        }

        public string GetStringMessage(T instance)
        {
            string strXmlMessage = "Could Not Serialize Message to Xml";

            DataContractSerializer dcs = new DataContractSerializer(typeof(T));

            using (MemoryStream ms = new MemoryStream())
            {
                dcs.WriteObject(ms, instance);

                strXmlMessage = GetStringMessage(ms, dcs, instance);

            }
            return strXmlMessage;
        }

        private string GetStringMessage(MemoryStream ms, DataContractSerializer dcs, T instance)
        {
            string returnMessage = "Could Not Determine the Message Contents";
            try
            {
                StringBuilder sb = new StringBuilder();
                using (XmlWriter xw = XmlWriter.Create(sb))
                {
                    dcs.WriteObject(xw, instance);
                }

                returnMessage = sb.ToString();

            }
            catch (Exception ex)
            {
                returnMessage = ex.StackTrace;
            }

            return returnMessage;
        }

        public T GetObjectFromString(string xml)
        {
            using (MemoryStream memoryStream = new MemoryStream(Encoding.BigEndianUnicode.GetBytes(xml)))
            {
                XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(memoryStream, Encoding.BigEndianUnicode, new XmlDictionaryReaderQuotas(), null);
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                return (T)serializer.ReadObject(reader);
            }

        }

        private static String UTF8ByteArrayToString(Byte[] characters)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            string constructedString = encoding.GetString(characters);
            return (constructedString);
        }

    }
}
