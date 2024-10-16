using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace L13Opg5
{
    // we need to use a Content Filter: removes unimportant data 
    // items from a message, leaving only important items
    internal class Program
    {
        private static MessageQueue checkInQueue;
        private static MessageQueue cargoQueue;
        private static ContentFilter contentFilter;
        private static Dictionary<string, double> cargoData;
        static void Main(string[] args)
        {
            // creating the two queues
            if (!MessageQueue.Exists(@".\Private$\L13AirportCheckInOutput"))
            {
                MessageQueue.Create(@".\Private$\L13AirportCheckInOutput");
            }
            checkInQueue = new MessageQueue(@".\Private$\L13AirportCheckInOutput");
            checkInQueue.Label = "CheckIn Queue";

            if (!MessageQueue.Exists(@".\Private$\L13AirportCargo"))
            {
                MessageQueue.Create(@".\Private$\L13AirportCargo");
            }
            cargoQueue = new MessageQueue(@".\Private$\L13AirportCargo");
            cargoQueue.Label = "Passenger Queue";

            // creating the content filter and data storage/dictionary
            contentFilter = new ContentFilter(cargoQueue);
            cargoData = new Dictionary<string, double>();

            // sending the check-in message
            SendCheckInMessage();

            Console.WriteLine("Listening for messages...");
            while (true) { }
        }

        // sending the XML message containing the check-in info
        private static void SendCheckInMessage()
        {
            XElement CheckInFile = XElement.Load(@"CheckedInPassenger.xml");
            Console.WriteLine("Original message:");
            Console.WriteLine(CheckInFile);
            Console.WriteLine("------------------------------");
            string AirlineCompany = "SAS";
            checkInQueue.Send(CheckInFile, AirlineCompany);

            // listening for incoming messages from the CheckInOutput queue
            checkInQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnMessageReceived);
            checkInQueue.BeginReceive();
        }

        private static void OnMessageReceived(object sender, ReceiveCompletedEventArgs e)
        {
            MessageQueue mq = (MessageQueue)sender;
            Message receivedMsg = mq.EndReceive(e.AsyncResult);

            // use the existing content filer instance to process the received message
            contentFilter.OnMessage(receivedMsg);

            // listening for incoming messages from the cargo queue
            cargoQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnCargoMessageReceived);
            cargoQueue.BeginReceive();

            // start listening for the next message
            mq.BeginReceive();
        }

        private static void OnCargoMessageReceived(object sender, ReceiveCompletedEventArgs e)
        {
            MessageQueue mq = (MessageQueue)sender;
            Message receivedMsg = mq.EndReceive(e.AsyncResult);

            double weight = Convert.ToDouble(receivedMsg.Label.Remove(0, 14));

            XmlDocument xml = new XmlDocument();
            string XMLDocument;
            using (Stream body = receivedMsg.BodyStream)
            using (StreamReader reader = new StreamReader(body))
            {
                XMLDocument = reader.ReadToEnd();
            }

            xml.LoadXml(XMLDocument);
            XmlNode flightDetails = xml.SelectSingleNode("CargoInfo/Flight");
            string flight = flightDetails.Attributes["number"].Value;

            if (!cargoData.ContainsKey(flight)) 
            {
                // add the flight to the dictionary
                cargoData.Add(flight, 0);
            }
            cargoData[flight] += weight;

            // print the cargo data for all flights
            foreach (KeyValuePair<string, double> kvp in cargoData)
            {
                Console.WriteLine("Flight = {0}, Total Cargo Weight = {1}", kvp.Key, kvp.Value);
            }
        }
    }
}
