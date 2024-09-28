using System;
using System.IO;
using System.Messaging;
using System.Xml;
using System.Xml.Linq;

namespace L10Opg3
{
    internal class Splitter
    {
        protected MessageQueue passengerQueue;
        protected MessageQueue luggageQueue;

        public Splitter(MessageQueue passengerQueue, MessageQueue luggageQueue)
        {
            this.passengerQueue = passengerQueue;
            this.luggageQueue = luggageQueue;
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
            XmlNode flightDetails = xml.SelectSingleNode("/FlightDetailsInfoResponse");

            if (flightDetails != null)
            {
                string reservationID = flightDetails.SelectSingleNode("Passenger/ReservationNumber").InnerText;
                XmlNode passenger = flightDetails.SelectSingleNode("Passenger");
                XmlNodeList luggageList = flightDetails.SelectNodes("Luggage");

                int totalMessages = 1 + luggageList.Count;
                SendPassengerMessage(passenger, reservationID, totalMessages);
                SendLuggageMessages(luggageList, flightDetails, reservationID, totalMessages);

                Console.WriteLine($"Message {message.Label} split and sent to Passenger and Luggage queues.");
            }
        }

        private void SendPassengerMessage(XmlNode passenger, string reservationID, int totalMessages)
        {
            XmlDocument passengerDoc = new XmlDocument();
            XmlElement passengerRoot = passengerDoc.CreateElement("PassengerInfo");
            passengerDoc.AppendChild(passengerRoot);
            passengerRoot.AppendChild(passengerDoc.ImportNode(passenger, true));

            SendMessage(passengerQueue, passengerDoc, reservationID, 1, totalMessages);
        }

        private void SendLuggageMessages(XmlNodeList luggageList, XmlNode flightDetails, string reservationID, int totalMessages)
        {
            int sequence = 2; // luggage starts with sequence 2
            foreach (XmlNode luggage in luggageList)
            {
                XmlDocument luggageDoc = new XmlDocument();
                XmlElement luggageRoot = luggageDoc.CreateElement("LuggageInfo");
                luggageDoc.AppendChild(luggageRoot);
                luggageRoot.AppendChild(luggageDoc.ImportNode(luggage, true));
                luggageRoot.AppendChild(luggageDoc.ImportNode(flightDetails.SelectSingleNode("Flight"), true));

                SendMessage(luggageQueue, luggageDoc, reservationID, sequence, totalMessages);
                sequence++;
            }
        }

        private void SendMessage(MessageQueue queue, XmlDocument xmlDocument, string reservationID, int sequence, int totalMessages)
        {
            Message msg = new Message
            {
                Body = xmlDocument,
                Label = $"{reservationID}-{sequence}/{totalMessages}",
                Formatter = new XmlMessageFormatter(new Type[] { typeof(XmlDocument) })
            };

            Console.WriteLine($"Sending message: {msg.Label}");
            queue.Send(msg);
        }
    }
}
