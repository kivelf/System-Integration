using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Messaging;

namespace L07Opg3
{
    public class AirportInfoETA
    {
        public String Airline_Id { get; set; }
        public String Flight_No { get; set; }
        public String From { get; set; }
        public String Aircraft { get; set; }
        public String Track_No { get; set; }
        public String Estimated_Arrival { get; set; }
    }
    public class Program
    {
        public static void Main(string[] args)
        {
            var SAS_AirportInfoETA = new AirportInfoETA
            {
                Airline_Id = "SAS",
                Flight_No = "SK952",
                From = "Amsterdam Schipol (AMS)",
                Aircraft = "Boeing 737-8K2",
                Track_No = "226",
                Estimated_Arrival = "11:45"
            };

            var KLM_AirportInfoETA = new AirportInfoETA
            {
                Airline_Id = "KLM",
                Flight_No = "KL1264",
                From = "Amsterdam Schipol (AMS)",
                Aircraft = "Boeing 737-8K2",
                Track_No = "123",
                Estimated_Arrival = "12:10"
            };

            var SW_AirportInfoETA = new AirportInfoETA
            {
                Airline_Id = "SW",
                Flight_No = "SW3345",
                From = "Stockholm (ARN)",
                Aircraft = "Boeing 737-8K2",
                Track_No = "65",
                Estimated_Arrival = "12:48"
            };

            List<String> airlines = new List<String>();
            airlines.Add("SAS");
            airlines.Add("SW");
            airlines.Add("KLM");

            List<MessageQueue> airlineMessageQueues = new List<MessageQueue>();

            MessageQueue messageQueue = null;
            // opret de 3 message queues
            foreach (var airline in airlines) 
            {
                if (!MessageQueue.Exists(@".\Private$\L07" + airline))
                {
                    // Opret Queue hvis den ikke eksisterer i forvejen
                    MessageQueue.Create(@".\Private$\L07" + airline);
                }
                messageQueue = new MessageQueue(@".\Private$\L07" + airline);
                messageQueue.Label = "L07" + airline;
                airlineMessageQueues.Add(messageQueue);
            }
            

            // sender SAS flight info
            string SASjsonString = JsonSerializer.Serialize(SAS_AirportInfoETA);
            string AirlineCompany = "SAS_ETA";
            foreach (var airlineQueue in airlineMessageQueues)
            {
                airlineQueue.Send(SASjsonString, AirlineCompany);
                // extra - udskriver beskeden i Console
                String message = airlineQueue.Receive().Body.ToString();
                Console.WriteLine(message + "\n------------------------------");
            }

            // sender SW flight info
            string SWjsonString = JsonSerializer.Serialize(SW_AirportInfoETA);
            AirlineCompany = "SW_ETA";
            foreach (var airlineQueue in airlineMessageQueues)
            {
                airlineQueue.Send(SWjsonString, AirlineCompany);
                Console.WriteLine("Besked sendt til subscriber " + airlineQueue.Label);
            }

            // sender KLM flight info
            string KLMjsonString = JsonSerializer.Serialize(KLM_AirportInfoETA);
            AirlineCompany = "KLM_ETA";
            foreach (var airlineQueue in airlineMessageQueues)
            {
                airlineQueue.Send(KLMjsonString, AirlineCompany);
            }

            Console.ReadLine();
        }
    }
}
