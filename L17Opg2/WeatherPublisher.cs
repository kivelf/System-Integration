using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

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

            inQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
            inQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnMessage);
            inQueue.BeginReceive();
        }

        private void OnMessage(Object source, ReceiveCompletedEventArgs asyncResult)
        {
            MessageQueue mq = (MessageQueue)source;
            Message message = mq.EndReceive(asyncResult.AsyncResult);
            string label = message.Label;

            // read the content from the original message once
            string XMLDocument;
            using (Stream body = message.BodyStream)
            using (StreamReader reader = new StreamReader(body))
            {
                XMLDocument = reader.ReadToEnd();
            }

            // send separate copies of the filtered message to each queue
            outQueueAirTrafficControlCenter.Send(FilterMessage(XMLDocument, "AirTrafficControlCenter"), label);
            Console.WriteLine("Message sent to " + outQueueAirTrafficControlCenter.Label);

            outQueueAirportInformationCenter.Send(FilterMessage(XMLDocument, "AirportInformationCenter"), label);
            Console.WriteLine("Message sent to " + outQueueAirportInformationCenter.Label);

            outQueueAirlineCompanies.Send(FilterMessage(XMLDocument, "AirlineCompanies"), label);
            Console.WriteLine("Message sent to " + outQueueAirlineCompanies.Label);

            mq.BeginReceive();
        }

        private Message FilterMessage(string XMLDocument, string receiver) 
        { 
            Message filteredMessage = new Message();
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(XMLDocument);
            XmlNode weatherInfo = xml.SelectSingleNode("/current");

            // common data
            XmlNode cityNode = weatherInfo.SelectSingleNode("/current/city");
            string cityName = cityNode?.Attributes["name"]?.Value;

            XmlDocument filteredDoc = new XmlDocument();
            XmlElement weatherRoot = filteredDoc.CreateElement("WeatherInfo");
            filteredDoc.AppendChild(weatherRoot);
            XmlElement newCityNode = filteredDoc.CreateElement("city");
            newCityNode.InnerText = cityName;
            weatherRoot.AppendChild(newCityNode);
            weatherRoot.AppendChild(filteredDoc.ImportNode(weatherInfo.SelectSingleNode("city/country"), true));
            weatherRoot.AppendChild(filteredDoc.ImportNode(weatherInfo.SelectSingleNode("temperature"), true));

            switch (receiver) 
            {
                case "AirTrafficControlCenter":
                    weatherRoot.AppendChild(filteredDoc.ImportNode(weatherInfo.SelectSingleNode("city/coord"), true));
                    weatherRoot.AppendChild(filteredDoc.ImportNode(weatherInfo.SelectSingleNode("humidity"), true));
                    weatherRoot.AppendChild(filteredDoc.ImportNode(weatherInfo.SelectSingleNode("pressure"), true));
                    weatherRoot.AppendChild(filteredDoc.ImportNode(weatherInfo.SelectSingleNode("wind"), true));
                    weatherRoot.AppendChild(filteredDoc.ImportNode(weatherInfo.SelectSingleNode("clouds"), true));
                    weatherRoot.AppendChild(filteredDoc.ImportNode(weatherInfo.SelectSingleNode("visibility"), true));

                    filteredMessage = new Message
                    {
                        Body = filteredDoc,
                        Label = "Weather Info for Air Traffic Control Center",
                        Formatter = new XmlMessageFormatter(new Type[] { typeof(XmlDocument) })
                    };
                    break;

                case "AirportInformationCenter":
                    weatherRoot.AppendChild(filteredDoc.ImportNode(weatherInfo.SelectSingleNode("city/sun"), true));

                    filteredMessage = new Message
                    {
                        Body = filteredDoc,
                        Label = "Weather Info for Airport Information Center",
                        Formatter = new XmlMessageFormatter(new Type[] { typeof(XmlDocument) })
                    };
                    break;

                case "AirlineCompanies":
                    weatherRoot.AppendChild(filteredDoc.ImportNode(weatherInfo.SelectSingleNode("clouds"), true));

                    filteredMessage = new Message {
                        Body = filteredDoc,
                        Label = "Weather Info for Airline Companies",
                        Formatter = new XmlMessageFormatter(new Type[] { typeof(XmlDocument) })
                    };
                    break;

                default:
                    Console.WriteLine("Unknown receiver!");
                    break;
            }

            return filteredMessage;
        }
    }
}
