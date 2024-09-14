using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Messaging;
using System.Collections;

namespace SysIntegration1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Opg. 2 og 3
            MessageQueue messageQueue = null;
            if (!MessageQueue.Exists(@".\Private$\TestQueue")) // tjek om kø er oprettet
            {
                
                MessageQueue.Create(@".\Private$\TestQueue");
                messageQueue = new MessageQueue(@".\Private$\TestQueue");
                messageQueue.Send("Besked sendt til MSMQ", "Titel");
                Console.WriteLine("Besked sendt til MSMQ");
            }
            else
            {
                // the queue already exists and we have a message in it, so we read it
                messageQueue = new MessageQueue(@".\Private$\TestQueue");
                var message = messageQueue.Receive();

                try
                {
                    MessageQueue.Delete(@".\Private$\TestQueue");
                }
                catch (MessageQueueException e)
                {
                    if (e.MessageQueueErrorCode ==
                        MessageQueueErrorCode.AccessDenied)
                    {
                        Console.WriteLine("Access is denied. " +
                            "Queue might be a system queue.");
                    }
                }
                
                if (message != null) Console.WriteLine(message.ToString());
            }
                
            Console.ReadLine();
        }
    }
}
