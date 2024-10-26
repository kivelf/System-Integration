using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace L14Opg1
{
    internal class Program
    {
        private static MessageQueue customsAndBorderProtectionQueue;
        private static RecipientList recipientList;
        static void Main(string[] args)
        {
            // creating the border queue
            if (!MessageQueue.Exists(@".\Private$\L14CustomsAndBorderProtectionQueue"))
            {
                MessageQueue.Create(@".\Private$\L14CustomsAndBorderProtectionQueue");
            }
            customsAndBorderProtectionQueue = new MessageQueue(@".\Private$\L14CustomsAndBorderProtectionQueue");
            customsAndBorderProtectionQueue.Label = "Customs & Border Protection Queue";

            // creating the recipient list
            recipientList = new RecipientList();

            // sending the passenger messages
            SendPassengerMessages();

            Console.WriteLine("Listening for messages...");
            while (true) { }
        }

        // sending the XML message containing the check-in info
        private static void SendPassengerMessages()
        {
            XElement PassengerFile = XElement.Load(@"Passenger1.xml");
            Console.WriteLine("Original message:");
            Console.WriteLine(PassengerFile);
            Console.WriteLine("------------------------------");
            customsAndBorderProtectionQueue.Send(PassengerFile, "Passenger 1");

            PassengerFile = XElement.Load(@"Passenger2.xml");
            Console.WriteLine("Original message:");
            Console.WriteLine(PassengerFile);
            Console.WriteLine("------------------------------");
            customsAndBorderProtectionQueue.Send(PassengerFile, "Passenger 2");

            // listening for incoming messages from the Customs & Border Protection queue
            customsAndBorderProtectionQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnMessageReceived);
            customsAndBorderProtectionQueue.BeginReceive();
        }

        private static void OnMessageReceived(object sender, ReceiveCompletedEventArgs e)
        {
            MessageQueue mq = (MessageQueue)sender;
            Message receivedMsg = mq.EndReceive(e.AsyncResult);

            // use the existing recipient list instance to process the received message
            recipientList.OnMessage(receivedMsg);

            // start listening for the next message
            mq.BeginReceive();
        }
    }
}
