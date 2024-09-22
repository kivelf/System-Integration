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
        private static MessageQueue resequencerQueue;
        static void Main(string[] args)
        {
            // creating the four queues
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

            if (!MessageQueue.Exists(@".\Private$\L10AirportResequencer"))
            {
                MessageQueue.Create(@".\Private$\L10AirportResequencer");
            }
            resequencerQueue = new MessageQueue(@".\Private$\L10AirportResequencer");
            resequencerQueue.Label = "Resequencer Queue";

            // create a list of input queues (passenger and luggage)
            List<MessageQueue> inputQueues = new List<MessageQueue> { passengerQueue, luggageQueue };

            // initialise the resequencer with both queues
            Resequencer resequencer = new Resequencer(inputQueues, resequencerQueue);

            // sending the message containing the check-in info (xml)
            SendCheckInMessage();

            Console.WriteLine("Listening for messages...");
            while (true) { }
        }

        // Sending the XML message containing the check-in info
        private static void SendCheckInMessage()
        {
            XElement CheckInFile = XElement.Load(@"CheckedInPassenger.xml");
            Console.WriteLine(CheckInFile);
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

            // split message into passenger and luggage queues
            Splitter splitter = new Splitter(passengerQueue, luggageQueue);
            splitter.OnMessage(receivedMsg);

            // start listening for the next message
            mq.BeginReceive();
        }
    }
}
