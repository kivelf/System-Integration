using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace L17Opg2
{
    class AirlinePublisher
    {
        protected MessageQueue inQueue;
        protected MessageQueue outQueueSAS;
        protected MessageQueue outQueueSouthWest;
        protected MessageQueue outQueueKLM;
        protected MessageQueue outQueueBritishAirways;

        public AirlinePublisher(MessageQueue inQueue, MessageQueue outQueueSAS, MessageQueue outQueueSouthWest, MessageQueue outQueueKLM, MessageQueue outQueueBritishAirways)
        {
            this.inQueue = inQueue;
            this.outQueueSAS = outQueueSAS;
            this.outQueueSouthWest = outQueueSouthWest;
            this.outQueueKLM = outQueueKLM;
            this.outQueueBritishAirways = outQueueBritishAirways;

            inQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
            inQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnMessage);
            inQueue.BeginReceive();
        }

        private void OnMessage(Object source, ReceiveCompletedEventArgs asyncResult)
        {
            MessageQueue mq = (MessageQueue)source;
            Message message = mq.EndReceive(asyncResult.AsyncResult);
            string label = message.Label;

            // read the XML content from the message
            string XMLDocument;
            using (Stream body = message.BodyStream)
            using (StreamReader reader = new StreamReader(body))
            {
                XMLDocument = reader.ReadToEnd();
            }

            // send a class-based message to SAS
            outQueueSAS.Send(TranslateMessage(XMLDocument, "SAS"), label);
            Console.WriteLine("Message sent from Airline Companies Channel to " + outQueueSAS.Label);

            // send a string-based message to KLM
            outQueueKLM.Send(TranslateMessage(XMLDocument, "KLM"), label);
            Console.WriteLine("Message sent from Airline Companies Channel to " + outQueueKLM.Label);

            // send the message as an XML-document to South West and British Airways
            Message SWMessage = new Message
            {
                Body = XMLDocument,
                Label = "Weather Info for South West Airlines",
                Formatter = new XmlMessageFormatter(new Type[] { typeof(string) })
            };
            outQueueSouthWest.Send(SWMessage, label);
            Console.WriteLine("Message sent from Airline Companies Channel to " + outQueueSouthWest.Label);

            Message BAMessage = new Message
            {
                Body = XMLDocument,
                Label = "Weather Info for British Airways",
                Formatter = new XmlMessageFormatter(new Type[] { typeof(string) })
            };
            outQueueBritishAirways.Send(BAMessage, label);
            Console.WriteLine("Message sent from Airline Companies Channel to " + outQueueBritishAirways.Label);

            mq.BeginReceive();
        }

        private Message TranslateMessage(string XMLDocument, string receiver)
        {
            Message translatedMessage = new Message();
            switch (receiver)
            {
                case "SAS":
                    // convert the XML document into a C# class object for SAS
                    var sasMessage = ConvertToClass(XMLDocument);
                    translatedMessage.Body = sasMessage;
                    translatedMessage.Label = "Weather Info for SAS";
                    translatedMessage.Formatter = new XmlMessageFormatter(new Type[] { typeof(MyWeatherClass) });
                    break;

                case "KLM":
                    // send the XML content as a plain string for KLM
                    translatedMessage.Body = XMLDocument;
                    translatedMessage.Label = "Weather Info for KLM";
                    translatedMessage.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                    break;

                default:
                    Console.WriteLine("Unknown airline!");
                    break;
            }

            return translatedMessage;
        }

        // method to convert the XML document into a class object (used by SAS)
        private MyWeatherClass ConvertToClass(string XMLDocument)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(XMLDocument);

            MyWeatherClass weatherData = new MyWeatherClass
            {
                CityName = xmlDoc.SelectSingleNode("/current/city")?.Attributes["name"]?.Value,
                Country = xmlDoc.SelectSingleNode("/current/city/country")?.InnerText,
                Temperature = xmlDoc.SelectSingleNode("/current/temperature")?.Attributes["value"]?.Value,
                Clouds = xmlDoc.SelectSingleNode("clouds")?.Attributes["value"]?.Value
            };

            return weatherData;
        }
    }

    public class MyWeatherClass
    {
        public string CityName { get; set; }
        public string Country { get; set; }
        public string Temperature { get; set; }
        public string Clouds { get; set; }
    }

}
