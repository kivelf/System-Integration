using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
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

            // creating the content filter
            contentFilter = new ContentFilter(cargoQueue);

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

            // start listening for the next message
            mq.BeginReceive();
        }
    }
}
