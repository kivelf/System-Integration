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
        private static MessageQueue USCitizenQueue;
        private static MessageQueue DKCitizenQueue;
        private static MessageQueue JPCitizenQueue;
        private static RecipientList recipientList;
        static void Main(string[] args)
        {
            // creating the four queues
            if (!MessageQueue.Exists(@".\Private$\L14CustomsAndBorderProtectionQueue"))
            {
                MessageQueue.Create(@".\Private$\L14CustomsAndBorderProtectionQueue");
            }
            customsAndBorderProtectionQueue = new MessageQueue(@".\Private$\L14CustomsAndBorderProtectionQueue");
            customsAndBorderProtectionQueue.Label = "Customs & Border Protection Queue";

            if (!MessageQueue.Exists(@".\Private$\L14USQueue"))
            {
                MessageQueue.Create(@".\Private$\L14USQueue");
            }
            USCitizenQueue = new MessageQueue(@".\Private$\L14USQueue");
            USCitizenQueue.Label = "US Citizen Queue";

            if (!MessageQueue.Exists(@".\Private$\L14DKQueue"))
            {
                MessageQueue.Create(@".\Private$\L14DKQueue");
            }
            DKCitizenQueue = new MessageQueue(@".\Private$\L14DKQueue");
            DKCitizenQueue.Label = "DK Citizen Queue";

            if (!MessageQueue.Exists(@".\Private$\L14JPQueue"))
            {
                MessageQueue.Create(@".\Private$\L14JPQueue");
            }
            JPCitizenQueue = new MessageQueue(@".\Private$\L14JPQueue");
            JPCitizenQueue.Label = "JP Citizen Queue";

            // creating the recipient list
            recipientList = new RecipientList();
            recipientList.AddRecipientChannel(USCitizenQueue);
            recipientList.AddRecipientChannel(DKCitizenQueue);
            recipientList.AddRecipientChannel(JPCitizenQueue);

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
