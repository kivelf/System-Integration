using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace L13Opg5
{
    internal class ContentFilter
    {
        protected MessageQueue cargoQueue;

        public ContentFilter(MessageQueue cargoQueue)
        { 
            this.cargoQueue = cargoQueue;
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

            double total = 0;


            if (flightDetails != null)
            {
                XmlNode flight = flightDetails.SelectSingleNode("Flight");
                XmlNode passenger = flightDetails.SelectSingleNode("Passenger");
                XmlNodeList luggageList = flightDetails.SelectNodes("Luggage");

                foreach (XmlNode node in luggageList) 
                { 
                    string weight = node.SelectSingleNode("Weight").InnerText;
                    total += Convert.ToDouble(weight);
                }

                SendCargoMessage(flight, passenger, total);

                Console.WriteLine($"Message {message.Label} total weight calculated and sent to Cargo queue.");
            }
        }

        private void SendCargoMessage(XmlNode flight, XmlNode passenger, double totalWeight)
        {
            XmlDocument cargoDoc = new XmlDocument();
            XmlElement cargoRoot = cargoDoc.CreateElement("CargoInfo");
            cargoDoc.AppendChild(cargoRoot);
            cargoRoot.AppendChild(cargoDoc.ImportNode(flight, true));
            cargoRoot.AppendChild(cargoDoc.ImportNode(passenger, true));

            Message msg = new Message
            {
                Body = cargoDoc,
                Label = $"Total weight: {totalWeight}",
                Formatter = new XmlMessageFormatter(new Type[] { typeof(XmlDocument) })
            };

            Console.WriteLine($"Sending message: {msg.Label}");
            cargoQueue.Send(msg);
        }
    }
}
