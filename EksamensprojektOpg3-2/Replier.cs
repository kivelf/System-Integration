using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EksamensprojektOpg3_2
{
    public class Replier
    {
        public Replier(String requestQueueName)
        {
            MessageQueue requestQueue = new MessageQueue(requestQueueName);

            requestQueue.MessageReadPropertyFilter.SetAll();
            requestQueue.Formatter = new XmlMessageFormatter(new string[] { "System.String,mscorlib" });

            try
            {
                requestQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnReceiveCompleted);
                requestQueue.BeginReceive();
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Something went wrong...");
            }
        }

        public void OnReceiveCompleted(Object source, ReceiveCompletedEventArgs asyncResult)
        {
            MessageQueue requestQueue = (MessageQueue)source;
            Message requestMessage = requestQueue.EndReceive(asyncResult.AsyncResult);
            requestMessage.Formatter = new XmlMessageFormatter(new string[] { "System.String,mscorlib" });

            try
            {
                // læser medlemsnummer fra beskeden
                string XMLDocument;
                using (Stream body = requestMessage.BodyStream)
                using (StreamReader reader = new StreamReader(body))
                {
                    XMLDocument = reader.ReadToEnd();
                }

                XmlDocument xml = new XmlDocument();
                xml.LoadXml(XMLDocument);
                string medlemsnummer = xml.SelectSingleNode("ForespoergselOmMedlemskab/Medlemsnummer").InnerText;

                // preparing to send back a reply
                MessageQueue replyQueue = requestMessage.ResponseQueue;

                string label = $"Medslemskabsstatus for {medlemsnummer}";
                string status = "";

                // hardcodede værdier - i virkeligheden vil man læse data fra DB'en
                switch (medlemsnummer)
                {
                    case "12345":
                        status = "true";
                        break;
                    case "22334":
                        status = "true";
                        break;
                    case "98765":
                        status = "false";
                        break;
                }

                // create the XmlDocument object
                XmlDocument xmlDocument = new XmlDocument();
                XmlElement forespoergselOmMedlemskab = xmlDocument.CreateElement("ForespoergselOmMedlemskab");
                XmlElement statusElement = xmlDocument.CreateElement("Status");
                statusElement.InnerText = status;

                // append the <Status> element to the root element
                forespoergselOmMedlemskab.AppendChild(statusElement);
                // append the root element to the XmlDocument
                xmlDocument.AppendChild(forespoergselOmMedlemskab);

                // opretter reply message
                Message replyMessage = new Message
                {
                    Body = xmlDocument,
                    Label = label,
                    Formatter = new XmlMessageFormatter(new string[] { "System.String,mscorlib" })
                };

                replyMessage.CorrelationId = requestMessage.Id;

                // udskriver Correlation ID (den bliver ikke brugt andre steder...)
                Console.WriteLine("replyMessage.CorrelationId == requestMessage.Id == " + replyMessage.CorrelationId.ToString());
                replyQueue.Send(replyMessage);
            }
            catch (Exception)
            {
                requestMessage.CorrelationId = requestMessage.Id;
            }

            // continue listening for messages
            requestQueue.BeginReceive();
        }
    }
}
