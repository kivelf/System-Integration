using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace L02Opg6
{
    class GateInfoRouter
    {
        protected MessageQueue inQueue;
        protected MessageQueue outQueue1; // SAS Queue
        protected MessageQueue outQueue2; // KLM Queue
        protected MessageQueue outQueue3; // SW Queue

        public GateInfoRouter(MessageQueue inQueue, MessageQueue outQueue1, MessageQueue outQueue2, MessageQueue outQueue3)
        {
            this.inQueue = inQueue;
            this.outQueue1 = outQueue1;
            this.outQueue2 = outQueue2;
            this.outQueue3 = outQueue3;
            inQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnMessage);
            inQueue.BeginReceive();
        }

        private void OnMessage(Object source, ReceiveCompletedEventArgs asyncResult)
        {
            MessageQueue mq = (MessageQueue)source;
            Message message = mq.EndReceive(asyncResult.AsyncResult);
            message.Formatter = new XmlMessageFormatter(new Type[] { typeof(GateInfo) });

            var gateInfo = (GateInfo)message.Body;

            // route based on the flight number prefix (f.ex., "SK" for SAS, "KL" for KLM)
            if (gateInfo.FlightNo.StartsWith("SK"))
            {
                outQueue1.Send(message); // SAS
            }
            else if (gateInfo.FlightNo.StartsWith("KL"))
            {
                outQueue2.Send(message); // KLM
            }
            else if (gateInfo.FlightNo.StartsWith("SW"))
            {
                outQueue3.Send(message); // South West Airlines
            }

            mq.BeginReceive();
        }
    }
}
