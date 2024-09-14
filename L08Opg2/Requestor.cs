using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Messaging;

namespace L08Opg2
{
    public class Requestor
    {
        private MessageQueue requestQueue;
        private MessageQueue replyQueue;
        private String Airline;

        public Requestor(String requestQueueName, String replyQueueName, String Airlines)
        {
            requestQueue = new MessageQueue(requestQueueName);
            replyQueue = new MessageQueue(replyQueueName);
            this.Airline = Airlines;

            replyQueue.Formatter = new XmlMessageFormatter(new string[] { "System.String,mscorlib" });
        }

        public void Send()
        {
            Message requestMessage = new Message();
            requestMessage.Formatter = new XmlMessageFormatter(new string[] { "System.String,mscorlib" });

            requestMessage.Body = Airline;
            requestMessage.Label = Airline.Substring(0, 2);

            requestMessage.ResponseQueue = replyQueue;
            try 
            {
                requestQueue.Send(requestMessage);
            }
            catch (MessageQueueException e) 
            {
                Console.WriteLine("Error sending message: " + e.Message);
            }
        }

        public void ReceiveSync()
        {
            Message replyMessage = new Message();
            try
            {
                replyMessage.Formatter = new XmlMessageFormatter(new string[] { "System.String,mscorlib" });
                string contents = (string)replyMessage.Body;
            }
            catch (MessageQueueException e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
