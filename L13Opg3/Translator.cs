using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace L13Opg3
{
    internal class Translator
    {
        protected MessageQueue outQueue;

        public Translator(MessageQueue outQueue)
        {
            this.outQueue = outQueue;
        }

        public void OnMessage(Message message)
        {
            String jsonString = message.Body.ToString();
            Console.WriteLine($"Json string: {jsonString}");

            string[] data = jsonString.Split('"');
            string airline = data[3];

            var translatedInfo = new AirlineFlightInfo();
            if (airline == "South West Airlines") 
            {
                // logic for translating the SW flight info message
                Console.WriteLine("Translating SW message");

                // process date and time
                string[] date = data[15].Split('/');
                string[] time = data[19].Split(' ');
                // "2024-10-02T15:30:00Z"
                string dateAndTime = date[2] + "-" + date[0] + "-" + date[1] + "T" + time[0] + ":00Z";

                translatedInfo = new AirlineFlightInfo 
                { 
                    Airline = airline,
                    FlightNo = data[7],
                    Origin = "N/A",
                    Destination = data[11],
                    ArrivalDeparture = "Departure",
                    DateAndTime = dateAndTime
                };
            }

            string SWjsonString = JsonSerializer.Serialize(translatedInfo);
            outQueue.Send(SWjsonString, "From: " + airline);
            Console.WriteLine("Message sent to " + outQueue.Label);
        }
    }
}
