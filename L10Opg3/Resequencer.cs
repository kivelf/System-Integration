using System;
using System.Collections.Generic;
using System.IO;
using System.Messaging;
using System.Xml;

namespace L10Opg3
{
    class Resequencer
    {
        private XmlNode[] messages;
        private string[] labels;
        private bool[] sentFlags;
        private int totalMessages = 0;
        private MessageQueue inputQueue;
        private MessageQueue resequencerQueue;
        private readonly object lockObject = new object(); // to ensure thread safety

        public Resequencer(MessageQueue inputQueue, MessageQueue resequencerQueue)
        {
            this.inputQueue = inputQueue;
            this.resequencerQueue = resequencerQueue;

            inputQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnMessageReceived);
            inputQueue.BeginReceive();
        }

        private void OnMessageReceived(object sender, ReceiveCompletedEventArgs e)
        {
            lock (lockObject) // ensure the method is thread-safe
            {
                try
                {
                    MessageQueue mq = (MessageQueue)sender;
                    Message receivedMsg = mq.EndReceive(e.AsyncResult);

                    Console.WriteLine("Resequencer received a message: " + receivedMsg.Label);
                    string label = receivedMsg.Label;
                    int sequence = ParseSequence(label);
                    totalMessages = ParseTotalMessages(label) - 1; // ensure total messages is updated (-1 because we don't process the passenger message)

                    // initialise arrays if they don't exist yet
                    if (messages == null || messages.Length < totalMessages)
                    {
                        messages = new XmlNode[totalMessages];
                        labels = new string[totalMessages];
                        sentFlags = new bool[totalMessages]; // track which messages have been sent from the buffer
                    }

                    // store the message body in the node it belongs to
                    XmlDocument xml = new XmlDocument();
                    using (Stream body = receivedMsg.BodyStream)
                    using (StreamReader reader = new StreamReader(body))
                    {
                        string XMLDocument = reader.ReadToEnd();
                        xml.LoadXml(XMLDocument);
                    }

                    // sequence - 2 because: 1) we don't process the passenger message, just luggage messages
                    // and 2) because the index of the array begins at 0, while messages begin at 1 :)
                    messages[sequence - 2] = xml.DocumentElement;
                    labels[sequence - 2] = label;

                    // process and send any complete sequences
                    ProcessBufferedMessages();

                    // listen for the next message
                    mq.BeginReceive();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in OnMessageReceived: {ex.Message}");
                }
            }
        }

        private void ProcessBufferedMessages()
        {
            lock (lockObject) // ensure the method is thread-safe
            {
                for (int i = 0; i < messages.Length; i++)
                {
                    if (messages[i] != null && !sentFlags[i]) // check if the message is not yet sent
                    {
                        try
                        {
                            Message resequencerMessage = new Message
                            {
                                Body = messages[i], // send the original XML body
                                Label = labels[i],  // also keeping the original label
                                Formatter = new XmlMessageFormatter(new Type[] { typeof(XmlDocument) })
                            };

                            resequencerQueue.Send(resequencerMessage);
                            Console.WriteLine($"Message sent by resequencer to AirportLuggageSort queue: {labels[i]}");

                            sentFlags[i] = true; // mark message as sent
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error sending from resequencer to AirportLuggageSort queue: {ex.Message}");
                        }
                    }
                }
            }
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
