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
        Dictionary<string, MessageQueue> recipients = new Dictionary<string, MessageQueue>();
        public RecipientList() { }

        public void AddRecipientChannel(MessageQueue recipient) 
        {
            string country = recipient.Label.Substring(0, 2);
            recipients.Add(country, recipient);
        }

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
                    string nationality = node.SelectSingleNode("Nationality").InnerText;
                    bool found = false;
                    while (!found) 
                    {
                        foreach (KeyValuePair<string, MessageQueue> entry in recipients) 
                        {
                            if (entry.Key == nationality) 
                            { 
                                found = true;
                                SendMessage(entry.Value, xml);
                            }
                        }
                        // ending the loop
                        found = true;
                    }
                }
            }
        }

        private void SendMessage(MessageQueue receiver, XmlDocument message)
        {
            Message msg = new Message
            {
                Body = message,
                Label = "From Bluff City International Airport Customs & Border Protection",
                Formatter = new XmlMessageFormatter(new Type[] { typeof(XmlDocument) })
            };

            Console.WriteLine($"Message sent to {receiver.Label}");
            receiver.Send(msg);
        }
    }
}
