using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Messaging;
using System.Security.Policy;
using System.Threading;

namespace L17Opg2
{
    internal class Program
    {
        // Enter your API key here.
        private const string API_KEY = "8c0a1499aadee9bcc9e8df577ac4faf8";

        // Query URLs. Replace @LOC@ with the location.
        private const string CurrentUrl =
            "http://api.openweathermap.org/data/2.5/weather?" +
            "q=@LOC@&mode=xml&units=metric&APPID=" + API_KEY;
        private const string ForecastUrl =
            "http://api.openweathermap.org/data/2.5/forecast?" +
            "q=@LOC@&mode=xml&units=metric&APPID=" + API_KEY;
        
        // queues
        private static MessageQueue dataFromAPIQueue;
        private static MessageQueue airTrafficControlCenterQueue;
        private static MessageQueue airportInformationCenterQueue;
        private static MessageQueue airlineCompaniesQueue;
        private static MessageQueue SASQueue;
        private static MessageQueue SWQueue;
        private static MessageQueue KLMQueue;
        private static MessageQueue BAQueue;

        // publishers
        private static WeatherPublisher weatherPublisher;
        private static AirlinePublisher airlinePublisher;

        static void Main(string[] args)
        {
            // creating the queues
            createQueues();

            // creating the publishers
            weatherPublisher = new WeatherPublisher(dataFromAPIQueue, airTrafficControlCenterQueue, airportInformationCenterQueue, airlineCompaniesQueue);
            airlinePublisher = new AirlinePublisher(airlineCompaniesQueue, SASQueue, SWQueue, KLMQueue, BAQueue);

            string url = CurrentUrl.Replace("@LOC@", "London");
            Console.WriteLine(GetFormattedXml(url));
            SendMessageToWeatherPublisherQueue(dataFromAPIQueue, GetXmlDoc(url));
            
            Console.WriteLine("------------------------------");
            Thread.Sleep(5000);
            url = CurrentUrl.Replace("@LOC@", "Tokyo");
            Console.WriteLine(GetFormattedXml(url));
            SendMessageToWeatherPublisherQueue(dataFromAPIQueue, GetXmlDoc(url));

            while (true) { }
        }

        // send data from the API to the In Queue
        private static void SendMessageToWeatherPublisherQueue(MessageQueue queue, XmlDocument message) 
        {
            Message msg = new Message
            {
                Body = message,
                Label = "From Weather Forecast API",
                Formatter = new XmlMessageFormatter(new Type[] { typeof(XmlDocument) })
            };

            queue.Send(msg);
        }

        // Return the XML result of the URL as a string
        private static string GetFormattedXml(string url)
        {
            // Create a web client.
            using (WebClient client = new WebClient())
            {
                // Get the response string from the URL.
                string xml = client.DownloadString(url);

                // Load the response into an XML document.
                XmlDocument xml_document = new XmlDocument();
                xml_document.LoadXml(xml);

                // Format the XML.
                using (StringWriter string_writer = new StringWriter())
                {
                    XmlTextWriter xml_text_writer =
                        new XmlTextWriter(string_writer);
                    xml_text_writer.Formatting = Formatting.Indented;
                    xml_document.WriteTo(xml_text_writer);

                    // Return the result.
                    return string_writer.ToString();
                }
            }
        }

        // Return the XML result of the URL as an XML-document
        private static XmlDocument GetXmlDoc(string url)
        {
            // Create a web client.
            using (WebClient client = new WebClient())
            {
                // Get the response string from the URL.
                string xml = client.DownloadString(url);

                // Load the response into an XML document.
                XmlDocument xml_document = new XmlDocument();
                xml_document.LoadXml(xml);

                return xml_document;
            }
        }

        private static void createQueues() 
        {
            if (!MessageQueue.Exists(@".\Private$\L17DataFromAPIQueue"))
            {
                MessageQueue.Create(@".\Private$\L17DataFromAPIQueue");
            }
            dataFromAPIQueue = new MessageQueue(@".\Private$\L17DataFromAPIQueue");
            dataFromAPIQueue.Label = "Data In From API Queue";

            if (!MessageQueue.Exists(@".\Private$\L17AirTrafficControlCenterQueue"))
            {
                MessageQueue.Create(@".\Private$\L17AirTrafficControlCenterQueue");
            }
            airTrafficControlCenterQueue = new MessageQueue(@".\Private$\L17AirTrafficControlCenterQueue");
            airTrafficControlCenterQueue.Label = "Air Traffic Control Center Queue";

            if (!MessageQueue.Exists(@".\Private$\L17AirportInformationCenterQueue"))
            {
                MessageQueue.Create(@".\Private$\L17AirportInformationCenterQueue");
            }
            airportInformationCenterQueue = new MessageQueue(@".\Private$\L17AirportInformationCenterQueue");
            airportInformationCenterQueue.Label = "Airport Information Center Queue";

            if (!MessageQueue.Exists(@".\Private$\L17AirlineCompaniesQueue"))
            {
                MessageQueue.Create(@".\Private$\L17AirlineCompaniesQueue");
            }
            airlineCompaniesQueue = new MessageQueue(@".\Private$\L17AirlineCompaniesQueue");
            airlineCompaniesQueue.Label = "Airline Companies Queue";

            if (!MessageQueue.Exists(@".\Private$\L17SASQueue"))
            {
                MessageQueue.Create(@".\Private$\L17SASQueue");
            }
            SASQueue = new MessageQueue(@".\Private$\L17SASQueue");
            SASQueue.Label = "SAS Queue";

            if (!MessageQueue.Exists(@".\Private$\L17SouthWestQueue"))
            {
                MessageQueue.Create(@".\Private$\L17SouthWestQueue");
            }
            SWQueue = new MessageQueue(@".\Private$\L17SouthWestQueue");
            SWQueue.Label = "South West Airline Queue";

            if (!MessageQueue.Exists(@".\Private$\L17KLMQueue"))
            {
                MessageQueue.Create(@".\Private$\L17KLMQueue");
            }
            KLMQueue = new MessageQueue(@".\Private$\L17KLMQueue");
            KLMQueue.Label = "KLM Queue";

            if (!MessageQueue.Exists(@".\Private$\L17BritishAirwaysQueue"))
            {
                MessageQueue.Create(@".\Private$\L17BritishAirwaysQueue");
            }
            BAQueue = new MessageQueue(@".\Private$\L17BritishAirwaysQueue");
            BAQueue.Label = "British Airways Queue";
        }
    }
}
