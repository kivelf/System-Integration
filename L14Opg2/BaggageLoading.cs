using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace L14Opg2
{
    internal class BaggageLoading
    {
        protected MessageQueue inQueue;
        protected MessageQueue controlQueue;
        private Dictionary<string, string> dynamicRules = new Dictionary<string, string>();

        public BaggageLoading(MessageQueue inQueue, MessageQueue controlQueue) 
        { 
            this.inQueue = inQueue;
            this.controlQueue = controlQueue;
            inQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnMessageInQueue);
            controlQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnMessageControlQueue);
            inQueue.BeginReceive();
            controlQueue.BeginReceive();
        }

        private async void OnMessageInQueue(Object source, ReceiveCompletedEventArgs asyncResult) 
        { 
            MessageQueue mq = (MessageQueue)source;
            Message msg = mq.EndReceive(asyncResult.AsyncResult);
            string lbl = msg.Label;
            XmlDocument xml = new XmlDocument();
            string XMLDocument = null;

            Stream body = msg.BodyStream;
            using (StreamReader reader = new StreamReader(body)) 
            { 
                XMLDocument = await reader.ReadToEndAsync();
            }
            xml.LoadXml(XMLDocument);

            string gateNr = xml.SelectSingleNode("/BaggageTransaction/GateNumber")?.InnerText;
            if (gateNr == null) 
            {
                Console.WriteLine("Gate number could not be retrieved!");
            }

            if (dynamicRules.ContainsKey(gateNr)) 
            { 
                SendMessage(dynamicRules[gateNr], xml);
            }

            mq.BeginReceive();
        }

        private async void OnMessageControlQueue(Object source, ReceiveCompletedEventArgs asyncResult)
        {
            MessageQueue mq = (MessageQueue)source;
            Message msg = mq.EndReceive(asyncResult.AsyncResult);
            string lbl = msg.Label;
            string messageContent = (string)msg.Body;
            string[] controlRules = messageContent.Split(new char[] { ',' });

            string gateNr = controlRules[0];
            string queuePath = controlRules[1];

            if (dynamicRules.ContainsKey(gateNr))
            {
                dynamicRules[gateNr] = queuePath;
            }
            else
            { 
                dynamicRules.Add(gateNr, queuePath);
            }
            mq.BeginReceive();
        }

        private void SendMessage(string path, XmlDocument xml) 
        {
            MessageQueue mq = null;
            string queuePath = $@".\Private$\L14{path}";
            Console.WriteLine("Baggage Loading Department sending message to " + queuePath);

            if (!MessageQueue.Exists(queuePath))
            {
                MessageQueue.Create(queuePath);
            }
            mq = new MessageQueue(queuePath);

            mq.Send(xml);
        }
    }
}
