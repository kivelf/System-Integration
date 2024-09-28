using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace L10Opg3
{
    internal class Aggregator
    {
        private XmlNode[] messages;
        private string label;
        private int totalMessages = 0;
        private int totalReceived = 0;
        private List<MessageQueue> inputQueues;
        private MessageQueue aggregatorQueue;
        private readonly object lockObject = new object(); // to ensure thread safety

        public Aggregator(List<MessageQueue> inputQueues, MessageQueue aggregatorQueue)
        {
            this.inputQueues = inputQueues;
            this.aggregatorQueue = aggregatorQueue;

            foreach (var queue in inputQueues)
            {
                queue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnMessageReceived);
                queue.BeginReceive();
            }
        }

        private void OnMessageReceived(object sender, ReceiveCompletedEventArgs e)
        {
            lock (lockObject) // ensure the method is thread-safe
            {
                try
                {
                    MessageQueue mq = (MessageQueue)sender;
                    Message receivedMsg = mq.EndReceive(e.AsyncResult);

                    Console.WriteLine("Aggregator received a message: " + receivedMsg.Label);
                    label = ParseLabel(receivedMsg.Label.ToString());
                    int sequence = ParseSequence(receivedMsg.Label.ToString());
                    totalMessages = ParseTotalMessages(receivedMsg.Label.ToString()); // ensure total messages is updated

                    // initialise arrays if they don't exist yet
                    if (messages == null || messages.Length < totalMessages)
                    {
                        messages = new XmlNode[totalMessages];
                    }

                    // store the message body in the node it belongs to
                    XmlDocument xml = new XmlDocument();
                    using (Stream body = receivedMsg.BodyStream)
                    using (StreamReader reader = new StreamReader(body))
                    {
                        string XMLDocument = reader.ReadToEnd();
                        xml.LoadXml(XMLDocument);
                    }
                    messages[sequence - 1] = xml.DocumentElement;
                    totalReceived++;

                    // process and send the complete message if all parts are present
                    if (totalMessages == totalReceived)
                    {
                        ProcessAggregatedMessages();
                    }
                    
                    // listen for the next message
                    mq.BeginReceive();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in OnMessageReceived: {ex.Message}");
                }
            }
        }

        private void ProcessAggregatedMessages()
        {
            lock (lockObject) // ensure the method is thread-safe
            {
                XmlDocument aggregatedPassengerDoc = new XmlDocument();
                XmlElement aggregatedRoot = aggregatedPassengerDoc.CreateElement("FlightDetailsInfoResponse");
                aggregatedPassengerDoc.AppendChild(aggregatedRoot);
                aggregatedRoot.AppendChild(aggregatedPassengerDoc.ImportNode(messages[1].SelectSingleNode("Flight"), true));
                aggregatedRoot.AppendChild(aggregatedPassengerDoc.ImportNode(messages[0], true));

                // attaching the luggage data
                for (int i = 1; i < messages.Length; i++)
                {
                    aggregatedRoot.AppendChild(aggregatedPassengerDoc.ImportNode(messages[i].SelectSingleNode("Luggage"), true));
                }

                try
                {
                    Message aggregatorMessage = new Message
                    {
                        Body = aggregatedPassengerDoc.OuterXml, // send the aggregated XML body
                        Label = label,  // also keeping the original label
                        Formatter = new ActiveXMessageFormatter()
                    };

                    aggregatorQueue.Send(aggregatorMessage);
                    Console.WriteLine($"Message sent to AggregatedPassenger queue: {label}");
                    Console.WriteLine(aggregatedPassengerDoc.OuterXml);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending to AggregatedPassenger queue: {ex.Message}");
                }
            }
        }

        private string ParseLabel(string label)
        {
            return label.Split('-')[0]; // extract label ID
        }

        private int ParseSequence(string label)
        {
            return int.Parse(label.Split('-')[1].Split('/')[0]); // extract sequence number
        }

        private int ParseTotalMessages(string label)
        {
            return int.Parse(label.Split('-')[1].Split('/')[1]); // extract total messages count
        }
    }
}
