using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace L14Opg1
{
    internal class RecipientList
    {
        public RecipientList() { }

        public void OnMessage(Message message)
        {
            XmlDocument xml = new XmlDocument();
            string XMLDocument;
            using (Stream body = message.BodyStream)
            using (StreamReader reader = new StreamReader(body))
            {
                XMLDocument = reader.ReadToEnd();
            }

            xml.LoadXml(XMLDocument);
            XmlNode passengerDetails = xml.SelectSingleNode("/CBPArrivalInfo");

            if (passengerDetails != null)
            {
                XmlNodeList passInfo = passengerDetails.SelectNodes("Passport");

                foreach (XmlNode node in passInfo)
                {
                    SendMessageToAllRecipients(node.SelectSingleNode("Nationality").InnerText, xml);
                }
            }
        }

        private void SendMessageToAllRecipients(string nationality, XmlDocument message)
        {
            Message msg = new Message
            {
                Body = message,
                Label = "From Bluff City International Airport Customs & Border Protection",
                Formatter = new XmlMessageFormatter(new Type[] { typeof(XmlDocument) })
            };

            switch (nationality) 
            {
                case ("DK"):
                    MessageQueue dkQueue = checkIfQueueExists(@".\Private$\L14DKQueue");
                    dkQueue.Send(msg);
                    Console.WriteLine("Message sent to DK Queue");
                    break;
                case ("US"):
                    MessageQueue usQueue = checkIfQueueExists(@".\Private$\L14USQueue");
                    usQueue.Send(msg);
                    Console.WriteLine("Message sent to US Queue");
                    break;
                case ("JP"):
                    MessageQueue jpQueue = checkIfQueueExists(@".\Private$\L14JPQueue");
                    jpQueue.Send(msg);
                    Console.WriteLine("Message sent to JP Queue");
                    break;
                default:
                    Console.WriteLine("Unknown nationality: " + nationality);
                    break;
            }
        }

        private static MessageQueue checkIfQueueExists(string path) 
        {
            if (!MessageQueue.Exists(path))
            {
                return MessageQueue.Create(path);
            }
            return new MessageQueue(path);
        }
    }
}
