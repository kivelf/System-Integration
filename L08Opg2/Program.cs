using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Messaging;

namespace L08Opg2
{
    class Program
    {
        static void Main(string[] args)
        {
            // creating the needed queues
            String Request = @".\Private$\BluffCityRequestQueueAIC";
            String ReplySAS = @".\Private$\BluffCityReplyQueueSAS";
            String ReplySW = @".\Private$\BluffCityReplyQueueSW";
            String ReplyKLM = @".\Private$\BluffCityReplyQueueKLM";
            String Invalid = @".\Private$\InvalidQueue";
            List<string> QueueList = new List<string>();
            QueueList.Add(Request);
            QueueList.Add(ReplySAS);
            QueueList.Add(ReplySW);
            QueueList.Add(ReplyKLM);
            QueueList.Add(Invalid);

            MessageQueue messageQueue = null;
            foreach (var queue in QueueList)
            {
                if (!MessageQueue.Exists(queue))
                {
                    // Opret Queue hvis den ikke eksisterer i forvejen
                    MessageQueue.Create(queue);
                }
                messageQueue = new MessageQueue(queue);
            }

            

            Requestor SAS = new Requestor(Request, ReplySAS, "SK249");
            Requestor SW = new Requestor(Request, ReplySW, "SW1423");
            Requestor KLM = new Requestor(Request, ReplyKLM, "KLM582");
            Replier AIC = new Replier(Request, Invalid);
            SAS.Send();
            SAS.ReceiveSync();
            KLM.Send();
            KLM.ReceiveSync();
            SW.Send();
            SW.ReceiveSync();

            Console.ReadLine();
        }
    }
}
