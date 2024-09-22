using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace L10Opg3
{
    class Resequencer
    {
        private List<MessageQueue> inputQueues;
        private MessageQueue outputQueue;
        private int startIndex = 1;  // the first message to send
        private Dictionary<int, Message> buffer = new Dictionary<int, Message>(); // buffer to store messages
        private int endIndex = -1;  // largest message index seen

        public Resequencer(List<MessageQueue> inputQueues, MessageQueue outputQueue)
        {
            this.inputQueues = inputQueues;
            this.outputQueue = outputQueue;

            foreach (var inputQueue in inputQueues)
            {
                // Setup message queue settings
                inputQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                inputQueue.MessageReadPropertyFilter.ClearAll();
                inputQueue.MessageReadPropertyFilter.AppSpecific = true;
                inputQueue.MessageReadPropertyFilter.Body = true;
                inputQueue.MessageReadPropertyFilter.CorrelationId = true;
                inputQueue.MessageReadPropertyFilter.Id = true;
                inputQueue.MessageReadPropertyFilter.Label = true; // Ensure Label is included

                // Start receiving messages from each queue
                inputQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnReceiveCompleted);
                inputQueue.BeginReceive();
            }

            Console.WriteLine("Processing messages from input queues to " + outputQueue.Path);
        }

        private void OnReceiveCompleted(object source, ReceiveCompletedEventArgs asyncResult)
        {
            MessageQueue mq = (MessageQueue)source;
            Message receivedMessage = mq.EndReceive(asyncResult.AsyncResult);

            // Ensure the formatter is correctly set
            receivedMessage.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
            ProcessMessage(receivedMessage);

            // Continue receiving the next message
            mq.BeginReceive();
        }

        private void ProcessMessage(Message m)
        {
            AddToBuffer(m);
            SendConsecutiveMessages();
        }

        private void AddToBuffer(Message m)
        {
            // Extract message index from the label
            var parts = m.Label.Split('-');
            if (parts.Length > 1 && parts[1].Contains("/"))
            {
                var sequencePart = parts[1].Split('/')[0]; // Get the sequence number
                if (int.TryParse(sequencePart, out int msgIndex))
                {
                    Console.WriteLine($"Received message index {msgIndex}");

                    // Store the message in the buffer and update endIndex
                    buffer[msgIndex] = m;
                    if (msgIndex > endIndex)
                    {
                        endIndex = msgIndex;
                    }
                }
            }
            else
            {
                Console.WriteLine("Invalid message label format.");
            }

            Console.WriteLine($"Buffer range: {startIndex} - {endIndex}");
        }

        private void SendConsecutiveMessages()
        {
            // Send messages in order based on their index
            while (buffer.ContainsKey(startIndex))
            {
                Message m = buffer[startIndex];  // Get the message with the correct index
                Console.WriteLine($"Sending message with index {startIndex}");

                try
                {
                    // Create a MemoryStream from the BodyStream
                    using (var memoryStream = new MemoryStream())
                    {
                        // Reset the BodyStream position before copying
                        m.BodyStream.Position = 0;
                        m.BodyStream.CopyTo(memoryStream);
                        memoryStream.Position = 0; // Reset position to the beginning

                        // Ensure the memory stream has data before loading
                        if (memoryStream.Length > 0)
                        {
                            // Load the XML from the memory stream
                            XmlDocument xmlDocument = new XmlDocument();
                            xmlDocument.Load(memoryStream);

                            // Log the XML being sent
                            Console.WriteLine($"Sending XML: {xmlDocument.OuterXml}");

                            // Create a new message for sending
                            Message newMessage = new Message
                            {
                                Body = xmlDocument, // Set the XML document as the body
                                Formatter = new XmlMessageFormatter(new Type[] { typeof(XmlDocument) }),
                                Label = m.Label // Keep the original label
                            };

                            outputQueue.Send(newMessage);
                        }
                        else
                        {
                            Console.WriteLine($"Error: Message with index {startIndex} has an empty body.");
                        }
                    }
                }
                catch (XmlException xmlEx)
                {
                    Console.WriteLine($"XML error processing message: {xmlEx.Message}");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");
                    break;
                }

                buffer.Remove(startIndex);  // Remove the sent message from the buffer
                startIndex++;  // Increment startIndex for the next message
            }

            // Log the current buffer state
            Console.WriteLine($"Current buffer size: {buffer.Count}, Start index: {startIndex}");
        }
    }

}
