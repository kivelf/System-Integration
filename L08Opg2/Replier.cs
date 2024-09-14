using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Messaging;

namespace L08Opg2
{
    class Replier
    {

        private MessageQueue invalidQueue;

        public Replier(String requestQueueName, String invalidQueueName)
        {
            MessageQueue requestQueue = new MessageQueue(requestQueueName);
            invalidQueue = new MessageQueue(invalidQueueName);

            requestQueue.MessageReadPropertyFilter.SetAll();
            requestQueue.Formatter = new XmlMessageFormatter(new string[] { "System.String,mscorlib" });

            try
            {
                requestQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnReceiveCompleted);
                requestQueue.BeginReceive();
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Something went wrong...");
            }
        }

        public void OnReceiveCompleted(Object source, ReceiveCompletedEventArgs asyncResult)
        {
            MessageQueue requestQueue = (MessageQueue)source;
            Message requestMessage = requestQueue.EndReceive(asyncResult.AsyncResult);
            requestMessage.Formatter = new XmlMessageFormatter(new string[] { "System.String,mscorlib" });

            try
            {
                string contents = (string)requestMessage.Body;
                MessageQueue replyQueue = requestMessage.ResponseQueue;
                Message replyMessage = new Message();
                replyMessage.Formatter = new XmlMessageFormatter(new string[] { "System.String,mscorlib" });
                string label = requestMessage.Label;
                switch (label)
                {
                    case "SK":
                        contents = "13:45";
                        break;
                    case "KL":
                        contents = "14:25";
                        break;
                    case "SW":
                        contents = "15:40";
                        break;
                }
                replyMessage.Label = label + " arrival time";
                replyMessage.Body = label + " plane ETA: " + contents;
                replyMessage.CorrelationId = requestMessage.Id;
                Console.WriteLine("replyMessage.CorrelationId == requestMessage.Id == " + replyMessage.CorrelationId.ToString());
                replyQueue.Send(replyMessage);
            }
            catch (Exception)
            {
                requestMessage.CorrelationId = requestMessage.Id;

                invalidQueue.Send(requestMessage);
            }

            requestQueue.BeginReceive();
        }
    }
}
