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
            string XMLDocument = null;
            Stream body = message.BodyStream;
            StreamReader reader = new StreamReader(body);
            XMLDocument = reader.ReadToEnd().ToString();
            xml.LoadXml(XMLDocument);

            // extract passenger and luggage data
            XmlNode flightDetails = xml.SelectSingleNode("/FlightDetailsInfoResponse");

            if (flightDetails != null)
            {
                // extract reservation number -- to be used as ID
                string reservationID = flightDetails.SelectSingleNode("Passenger/ReservationNumber").InnerText;

                // split passenger and luggage data
                XmlNode passenger = flightDetails.SelectSingleNode("Passenger");
                XmlNodeList luggageList = flightDetails.SelectNodes("Luggage");

                // calculate total number of messages (passenger + luggage)
                int totalMessages = 1 + luggageList.Count;  // 1 for the passenger, the rest for luggage

                // create and send passenger message
                XmlDocument passengerDoc = new XmlDocument();
                XmlElement passengerRoot = passengerDoc.CreateElement("PassengerInfo");
                passengerDoc.AppendChild(passengerRoot);

                // add passenger details
                XmlNode importedPassenger = passengerDoc.ImportNode(passenger, true);
                passengerRoot.AppendChild(importedPassenger);

                // if no luggage, add flight info to passenger
                if (luggageList.Count == 0)
                {
                    XmlNode flight = flightDetails.SelectSingleNode("Flight");
                    XmlNode importedFlight = passengerDoc.ImportNode(flight, true);
                    passengerRoot.AppendChild(importedFlight);
                }

                // send passenger message (sequence = 1)
                SendMessage(passengerQueue, passengerDoc, reservationID, 1, totalMessages);

                // send luggage messages
                int sequence = 2; // luggage starts with sequence 2
                foreach (XmlNode luggage in luggageList)
                {
                    XmlDocument luggageDoc = new XmlDocument();
                    XmlElement luggageRoot = luggageDoc.CreateElement("LuggageInfo");
                    luggageDoc.AppendChild(luggageRoot);

                    // add luggage details and flight info
                    XmlNode importedLuggage = luggageDoc.ImportNode(luggage, true);
                    luggageRoot.AppendChild(importedLuggage);

                    XmlNode flight = flightDetails.SelectSingleNode("Flight");
                    XmlNode importedFlight = luggageDoc.ImportNode(flight, true);
                    luggageRoot.AppendChild(importedFlight);

                    // send luggage message with incremented sequence
                    SendMessage(luggageQueue, luggageDoc, reservationID, sequence, totalMessages);
                    sequence++;
                }

                Console.WriteLine("Message split and sent to Passenger and Luggage queues.");
            }
        }

        // helper method to send messages with XmlDocument and sequence info
        private void SendMessage(MessageQueue queue, XmlDocument xmlDocument, string reservationID, int sequence, int totalMessages)
        {
            Message msg = new Message();

            // set the body to be the XML document
            msg.Body = xmlDocument;

            // label format: reservationID-sequence/total (f.ex., CA937200305251-1/3)
            msg.Label = $"{reservationID}-{sequence}/{totalMessages}";

            // use XmlMessageFormatter with XmlDocument type to ensure it's sent as XML and not as a string!
            msg.Formatter = new XmlMessageFormatter(new Type[] { typeof(XmlDocument) });
            queue.Send(msg);
        }
    }
}
