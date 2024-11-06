using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace L17Opg2
{
    class WeatherPublisher
    {
        protected MessageQueue inQueue;
        protected MessageQueue outQueueAirTrafficControlCenter;
        protected MessageQueue outQueueAirportInformationCenter;
        protected MessageQueue outQueueAirlineCompanies;

        public WeatherPublisher(MessageQueue inQueue, MessageQueue outQueueAirTrafficControl, MessageQueue outQueueAirportInfo, MessageQueue outQueueAirlines)
        {
            this.inQueue = inQueue;
            this.outQueueAirTrafficControlCenter = outQueueAirTrafficControl;
            this.outQueueAirportInformationCenter = outQueueAirportInfo;
            this.outQueueAirlineCompanies = outQueueAirlines;

            inQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnMessage);
            inQueue.BeginReceive();
        }

        private void OnMessage(Object source, ReceiveCompletedEventArgs asyncResult)
        {
            MessageQueue mq = (MessageQueue)source;
            Message message = mq.EndReceive(asyncResult.AsyncResult);
            string label = message.Label;
            outQueueAirTrafficControlCenter.Send(message);
            outQueueAirportInformationCenter.Send(message);
            outQueueAirlineCompanies.Send(message);

            mq.BeginReceive();
        }
    }
}
