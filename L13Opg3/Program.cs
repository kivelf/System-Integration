using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Messaging;
using System.Xml.Linq;

/*
 * Opgave 2: Canonical Data Model for flight information
 * coming from the 3 airline companies (using JSON):
 * 
 * {
    "AirlineFlightInfo": 
      {
        "Airline": "string",
        "FlightNo": "string",
        "Origin": "string",
        "Destination": "string",
        "ArrivalDeparture": "string",
        "DateAndTime": "string"  
        // in JSON there is no native DateTime type
        // so we use a string in the ISO 8601 standard (YYYY-MM-DDTHH:MM:SSZ)
      }
    }
 */

namespace L13Opg3
{
    public class AirlineFlightInfo
    {
        public String Airline { get; set; }
        public String FlightNo { get; set; }
        public String Origin { get; set; }
        public String Destination { get; set; }
        public String ArrivalDeparture { get; set; }
        public String DateAndTime { get; set; }
    }

    public class AirlineCompanySW
    {
        public String Airline { get; set; } //South West Airlines
        public String FlightNo { get; set; } //SW056
        public String Destination { get; set; } //New York
        public String Date { get; set; } //03/06/2017
        public String Departure { get; set; } //09:45 PM
    }


    internal class Program
    {
        private static MessageQueue messageQueue;
        private static MessageQueue translatedMessageQueue;
        private static Translator translator;
        public static void Main(string[] args)
        {
            var SWFlightInfo = new AirlineCompanySW 
            { 
                Airline = "South West Airlines",
                FlightNo = "SW056",
                Destination = "New York",
                Date = "03/06/2017",
                Departure = "09:45 PM"
            };

            // message queue that receives the flight info from the 3 companies
            if (!MessageQueue.Exists(@".\Private$\L13FlightInfoQueue"))
            {
                // Opret Queue hvis den ikke eksisterer i forvejen
                MessageQueue.Create(@".\Private$\L13FlightInfoQueue");
            }
            messageQueue = new MessageQueue(@".\Private$\L13FlightInfoQueue");
            messageQueue.Label = "FlightInfoQueue";

            // queue that receives the translated messages
            if (!MessageQueue.Exists(@".\Private$\L13TranslatedFlightInfoQueue"))
            {
                MessageQueue.Create(@".\Private$\L13TranslatedFlightInfoQueue");
            }
            translatedMessageQueue = new MessageQueue(@".\Private$\L13TranslatedFlightInfoQueue");
            translatedMessageQueue.Label = "Translated Flight Info Queue";

            // create the translator and pass the input and output queue
            translator = new Translator(translatedMessageQueue);

            // sender SW flight info to the queue
            string SWjsonString = JsonSerializer.Serialize(SWFlightInfo);
            string AirlineCompany = "SW";
            messageQueue.Send(SWjsonString, AirlineCompany);

            // listening for incoming messages from the CheckInOutput queue
            messageQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnMessageReceived);
            messageQueue.BeginReceive();

            Console.WriteLine("Listening for messages...");
            while (true) { }
        }

        private static void OnMessageReceived(object sender, ReceiveCompletedEventArgs e)
        {
            MessageQueue mq = (MessageQueue)sender;
            Message receivedMsg = mq.EndReceive(e.AsyncResult);

            // use the existing translator instance to process the received message
            translator.OnMessage(receivedMsg);

            // start listening for the next message
            mq.BeginReceive();
        }
    }
}
