using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace L16Opg1
{
    internal class Program
    {
        private static MessageQueue messageQueue;
        private static MessageQueue passengerQueue;
        private static MessageQueue luggageQueue;
        private static Splitter splitter;
        static void Main(string[] args)
        {
            // creating the three queues
            if (!MessageQueue.Exists(@".\Private$\L16AirportCheckInOutput"))
            {
                MessageQueue.Create(@".\Private$\L16AirportCheckInOutput");
            }
            messageQueue = new MessageQueue(@".\Private$\L16AirportCheckInOutput");
            messageQueue.Label = "CheckIn Queue";

            if (!MessageQueue.Exists(@".\Private$\L16AirportPassenger"))
            {
                MessageQueue.Create(@".\Private$\L16AirportPassenger", true);   // transactional queue
            }
            passengerQueue = new MessageQueue(@".\Private$\L16AirportPassenger");
            passengerQueue.Label = "Passenger Queue";

            if (!MessageQueue.Exists(@".\Private$\L16AirportLuggage"))
            {
                MessageQueue.Create(@".\Private$\L16AirportLuggage", true);   // transactional queue
            }
            luggageQueue = new MessageQueue(@".\Private$\L16AirportLuggage");
            luggageQueue.Label = "Luggage Queue";

            // create a single splitter instance
            splitter = new Splitter(passengerQueue, luggageQueue);

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
            messageQueue.Send(CheckInFile, AirlineCompany);

            // listening for incoming messages from the CheckInOutput queue
            messageQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnMessageReceived);
            messageQueue.BeginReceive();
        }

        private static void OnMessageReceived(object sender, ReceiveCompletedEventArgs e)
        {
            MessageQueue mq = (MessageQueue)sender;
            Message receivedMsg = mq.EndReceive(e.AsyncResult);

            // use the existing splitter instance to process the received message
            splitter.OnMessage(receivedMsg);
        }
    }
}
