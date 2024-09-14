using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace SysIntegration1
{
    class SimpleRouter
    {
        protected MessageQueue inQueue;
        protected MessageQueue outQueue1;
        protected MessageQueue outQueue2;

        public SimpleRouter(MessageQueue inQueue, MessageQueue outQueue1, MessageQueue outQueue2)
        {
            this.inQueue = inQueue;
            this.outQueue1 = outQueue1;
            this.outQueue2 = outQueue2;
            inQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnMessage);
            inQueue.BeginReceive();
        }

        private void OnMessage(Object source, ReceiveCompletedEventArgs asyncResult)
        {
            MessageQueue mq = (MessageQueue)source;
            Message message = mq.EndReceive(asyncResult.AsyncResult);
            message.Formatter = new XmlMessageFormatter(new Type[] { typeof(FlightInfo) });

            var flightInfo = (FlightInfo)message.Body;

            // simple routing logic: route based on airline name
            if (flightInfo.Airline == "Scandinavian Airline Service")
                outQueue1.Send(message);
            else if (flightInfo.Airline == "South West Airlines")
                outQueue2.Send(message);

            mq.BeginReceive();
        }
    }

    [Serializable]
    public class FlightInfo
    {
        public string Airline { get; set; }
        public string FlightNumber { get; set; }
        public string ScheduledTime { get; set; }
        public string Destination { get; set; }
        public bool CheckIn { get; set; }
    }
}