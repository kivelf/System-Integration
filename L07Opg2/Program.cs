using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Messaging;

namespace L07Opg2
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

            MessageQueue messageQueue = null;
            if (!MessageQueue.Exists(@".\Private$\L07AirportInfoETA"))
            {
                // Opret Queue hvis den ikke eksisterer i forvejen
                MessageQueue.Create(@".\Private$\L07AirportInfoETA");
            }
            messageQueue = new MessageQueue(@".\Private$\L07AirportInfoETA");
            messageQueue.Label = "AirportInfoETAQueue";

            // sender SAS flight info
            string SASjsonString = JsonSerializer.Serialize(SAS_AirportInfoETA);
            string AirlineCompany = "SAS_ETA";
            messageQueue.Send(SASjsonString, AirlineCompany);

            String message = messageQueue.Receive().Body.ToString();
            Console.WriteLine(message);
            

            // sender KLM flight info
            string KLMjsonString = JsonSerializer.Serialize(KLM_AirportInfoETA);
            AirlineCompany = "KLM_ETA";
            messageQueue.Send(KLMjsonString, AirlineCompany);

            message = messageQueue.Receive().Body.ToString();
            Console.WriteLine(message);
            Console.ReadLine();
        }
    }
}
