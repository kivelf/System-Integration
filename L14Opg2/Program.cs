using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace L14Opg2
{
    internal class Program
    {
        // using Dynamic Router pattern
        private static MessageQueue inQueue;
        private static MessageQueue controlQueue;
        private static BaggageLoading baggageLoading;
        static void Main(string[] args)
        {
            // creating the two queues
            if (!MessageQueue.Exists(@".\Private$\L14inQueue"))
            {
                MessageQueue.Create(@".\Private$\L14inQueue");
            }
            inQueue = new MessageQueue(@".\Private$\L14inQueue");
            inQueue.Label = "In Queue";

            if (!MessageQueue.Exists(@".\Private$\L14controlQueue"))
            {
                MessageQueue.Create(@".\Private$\L14controlQueue");
            }
            controlQueue = new MessageQueue(@".\Private$\L14controlQueue");
            controlQueue.Label = "Control Queue";

            // creating the baggage loading department
            baggageLoading = new BaggageLoading(inQueue, controlQueue);

            SASProgram(controlQueue);
            SWProgram(controlQueue);

            XElement baggage1 = XElement.Load(@"Baggagetransaction1.xml");
            XElement baggage2 = XElement.Load(@"Baggagetransaction2.xml");
            XElement baggage3 = XElement.Load(@"Baggagetransaction3.xml");

            inQueue.Send(baggage1);
            Console.WriteLine("Baggage sent to Gate 14");

            inQueue.Send(baggage2);
            Console.WriteLine("Baggage sent to Gate 3");

            inQueue.Send(baggage3);
            Console.WriteLine("Baggage sent to Gate 14");

            while (true) { }
        }

        static void SASProgram(MessageQueue controlQueue) 
        {
            Console.WriteLine("SAS requests all baggages for Gate 3");

            if (!MessageQueue.Exists(@".\Private$\L14SASQueue"))
            {
                MessageQueue.Create(@".\Private$\L14SASQueue");
            }

            controlQueue.Send("3,SAS");
        }

        static void SWProgram(MessageQueue controlQueue)
        {
            Console.WriteLine("SW requests all baggages for Gate 14");

            if (!MessageQueue.Exists(@".\Private$\L14SWQueue"))
            {
                MessageQueue.Create(@".\Private$\L14SWQueue");
            }

            controlQueue.Send("14,SW");
        }
    }
}
