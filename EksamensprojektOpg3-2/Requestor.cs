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
    public class Requestor
    {
        private MessageQueue requestQueue;
        private MessageQueue replyQueue;
        private String Medlemsnummer;

        public Requestor(String requestQueueName, String replyQueueName, String medlemsnummer)
        {
            requestQueue = new MessageQueue(requestQueueName);
            replyQueue = new MessageQueue(replyQueueName);
            this.Medlemsnummer = medlemsnummer;

            replyQueue.Formatter = new XmlMessageFormatter(new string[] { "System.String,mscorlib" });
            replyQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnMessage);
            replyQueue.BeginReceive();
        }

        public void Send()
        {
            // create the XmlDocument object
            XmlDocument xmlDocument = new XmlDocument();
            XmlElement forespoergselOmMedlemskab = xmlDocument.CreateElement("ForespoergselOmMedlemskab");
            XmlElement medlemsnummerElement = xmlDocument.CreateElement("Medlemsnummer");
            medlemsnummerElement.InnerText = Medlemsnummer;

            // append the <Medlemsnummer> element to the root element
            forespoergselOmMedlemskab.AppendChild(medlemsnummerElement);
            // append the root element to the XmlDocument
            xmlDocument.AppendChild(forespoergselOmMedlemskab);

            Message requestMessage = new Message
            {
                Body = xmlDocument,
                Label = "Request fra Tilmeldingssystemmet",
                Formatter = new XmlMessageFormatter(new string[] { "System.String,mscorlib" })
            };

            requestMessage.ResponseQueue = replyQueue;
            try
            {
                requestQueue.Send(requestMessage);
            }
            catch (MessageQueueException e)
            {
                Console.WriteLine("Error sending message: " + e.Message);
            }
        }

        public void OnMessage(Object source, ReceiveCompletedEventArgs asyncResult)
        {
            MessageQueue mq = (MessageQueue)source;

            Message message = mq.EndReceive(asyncResult.AsyncResult);
            // fjerner 'Medslemskabsstatus for ' fra label for at læse medlemsnummeret
            string medlemsnummer = message.Label.Substring(23);

            // read the content from the original message
            string XMLDocument;
            using (Stream body = message.BodyStream)
            using (StreamReader reader = new StreamReader(body))
            {
                XMLDocument = reader.ReadToEnd();
            }

            // læser status fra reply beskeden
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(XMLDocument);
            string status = xml.SelectSingleNode("ForespoergselOmMedlemskab/Status").InnerText;

            if (status.Equals("true"))
            {
                Console.WriteLine($"Medlem {medlemsnummer} har et aktivt medlemskab, og derfor er prisen for en billet 500 DKK.\n");
            }
            else
            {
                // ikke medlem
                Console.WriteLine($"Medlem {medlemsnummer} har ikke et aktivt medlemskab, og derfor er prisen for en billet 750 DKK.\n");
            }
        }
    }
}
