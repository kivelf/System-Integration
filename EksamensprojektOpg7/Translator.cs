using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EksamensprojektOpg7
{
    internal class Translator
    {
        protected MessageQueue InQueue;
        protected MessageQueue OutQueue;

        public Translator(MessageQueue inQueue, MessageQueue outQueue)
        {
            InQueue = inQueue;
            OutQueue = outQueue;

            InQueue.Formatter = new XmlMessageFormatter(new string[] { "System.String,mscorlib" });
            InQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnMessage);
            InQueue.BeginReceive();
        }

        public void OnMessage(Object source, ReceiveCompletedEventArgs asyncResult)
        {
            // modtager beskeden
            MessageQueue mq = (MessageQueue)source;
            Message message = mq.EndReceive(asyncResult.AsyncResult);

            // read the content from the original message
            string XMLDocument;
            using (Stream body = message.BodyStream)
            using (StreamReader reader = new StreamReader(body))
            {
                XMLDocument = reader.ReadToEnd();
            }

            // extracts the needed data
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(XMLDocument);
            string nummer = xml.SelectSingleNode("Medlem/Medlemsnummer").InnerText;
            string fornavn = xml.SelectSingleNode("Medlem/Fornavn").InnerText;
            string efternavn = xml.SelectSingleNode("Medlem/Efternavn").InnerText;
            string adresse = xml.SelectSingleNode("Medlem/Adresse").InnerText;
            string by = xml.SelectSingleNode("Medlem/ByNavn").InnerText;
            string dato = xml.SelectSingleNode("Medlem/DatoBrev").InnerText;

            // translates the data
            // create the XmlDocument object
            XmlDocument xmlDocument = new XmlDocument();
            // root element
            XmlElement memberElement = xmlDocument.CreateElement("Member");
            // children elements
            XmlElement membershipNumberElement = xmlDocument.CreateElement("MembershipNumber");
            membershipNumberElement.InnerText = nummer;
            XmlElement nameElement = xmlDocument.CreateElement("Name");
            nameElement.InnerText = fornavn + " " + efternavn;
            XmlElement addressElement = xmlDocument.CreateElement("Address");
            addressElement.InnerText = adresse + ",\n " + by;
            XmlElement zipCodeElement = xmlDocument.CreateElement("ZipCode");
            zipCodeElement.InnerText = xml.SelectSingleNode("Medlem/PostNr").InnerText;
            XmlElement mailElement = xmlDocument.CreateElement("Mail");
            mailElement.InnerText = xml.SelectSingleNode("Medlem/MailAdresse").InnerText;
            XmlElement dateElement = xmlDocument.CreateElement("DateRegistered");
            dateElement.InnerText = ConvertDanishToAmericanDate(dato);

            // append children elements to the root element
            memberElement.AppendChild(membershipNumberElement);
            memberElement.AppendChild(nameElement);
            memberElement.AppendChild(addressElement);
            memberElement.AppendChild(zipCodeElement);
            memberElement.AppendChild(mailElement);
            memberElement.AppendChild(dateElement);
            // append the root element to the XmlDocument
            xmlDocument.AppendChild(memberElement);

            // sends the translated message to the receiving channel
            Message subscribeMessage = new Message
            {
                Body = xmlDocument,
                Label = "New member for the Newsletter",
                Formatter = new XmlMessageFormatter(new string[] { "System.String,mscorlib" })
            };

            // send to the out queue
            try
            {
                OutQueue.Send(subscribeMessage);
            }
            catch (MessageQueueException e)
            {
                Console.WriteLine("Error sending message: " + e.Message);
            }

            // continue listening for messages
            InQueue.BeginReceive();
        }

        public static string ConvertDanishToAmericanDate(string danishDate)
        {
            var danishCulture = new CultureInfo("da-DK");

            // try to parse the Danish date string into a DateTime
            if (DateTime.TryParseExact(danishDate, "dd. MMMM yyyy", danishCulture, DateTimeStyles.None, out DateTime parsedDate))
            {
                // format the successfully parsed DateTime as MM/dd/yyyy f.ex. 10/23/2024
                return parsedDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            }
            else
            {
                throw new FormatException($"The date '{danishDate}' is not in the expected format 'dd. MMMM yyyy'!");
            }
        }
    }
}
