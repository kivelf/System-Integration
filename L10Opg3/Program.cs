using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Messaging;
using System.Collections;
using System.Threading;

namespace L10Opg3
{
    internal class Program
    {
        private static MessageQueue messageQueue;
        private static MessageQueue passengerQueue;
        private static MessageQueue luggageQueue;
        private static MessageQueue luggageSortQueue;
        private static MessageQueue aggregatedPassengerQueue;
        private static Splitter splitter;
        private static Resequencer resequencer;
        private static Aggregator aggregator;

        static void Main(string[] args)
        {
            // creating the five queues
            if (!MessageQueue.Exists(@".\Private$\L10AirportCheckInOutput"))
            {
                MessageQueue.Create(@".\Private$\L10AirportCheckInOutput");
            }
            messageQueue = new MessageQueue(@".\Private$\L10AirportCheckInOutput");
            messageQueue.Label = "CheckIn Queue";

            if (!MessageQueue.Exists(@".\Private$\L10AirportPassenger"))
            {
                MessageQueue.Create(@".\Private$\L10AirportPassenger");
            }
            passengerQueue = new MessageQueue(@".\Private$\L10AirportPassenger");
            passengerQueue.Label = "Passenger Queue";

            if (!MessageQueue.Exists(@".\Private$\L10AirportLuggage"))
            {
                MessageQueue.Create(@".\Private$\L10AirportLuggage");
            }
            luggageQueue = new MessageQueue(@".\Private$\L10AirportLuggage");
            luggageQueue.Label = "Luggage Queue";

            if (!MessageQueue.Exists(@".\Private$\L10AirportLuggageSort"))
            {
                MessageQueue.Create(@".\Private$\L10AirportLuggageSort");
            }
            luggageSortQueue = new MessageQueue(@".\Private$\L10AirportLuggageSort");
            luggageSortQueue.Label = "Luggage Sort Queue";

            if (!MessageQueue.Exists(@".\Private$\L10AggregatedPassenger"))
            {
                MessageQueue.Create(@".\Private$\L10AggregatedPassenger");
            }
            aggregatedPassengerQueue = new MessageQueue(@".\Private$\L10AggregatedPassenger");
            aggregatedPassengerQueue.Label = "Aggregated Passenger Queue";


            // create a list of input queues (passenger and luggage) - used by the aggregator
            List<MessageQueue> inputQueues = new List<MessageQueue> { passengerQueue, luggageSortQueue };

            // create a single splitter instance
            splitter = new Splitter(passengerQueue, luggageQueue);

            // create the resequencer and pass the input and output queue
            resequencer = new Resequencer(luggageQueue, luggageSortQueue);

            // create the aggregator and pass the input and output queue
            aggregator = new Aggregator(inputQueues, aggregatedPassengerQueue);

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

            // start listening for the next message
            mq.BeginReceive();
        }
    }
}
