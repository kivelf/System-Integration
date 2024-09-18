using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace L09Opg3
{
    public class AirportInfoETA
    {
        public String Airline_Id { get; set; }
        public String Flight_No { get; set; }
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
                Estimated_Arrival = "11:47"
            };

            var KLM_AirportInfoETA = new AirportInfoETA
            {
                Airline_Id = "KLM",
                Flight_No = "KL1264",
                Estimated_Arrival = "12:18"
            };

            var SW_AirportInfoETA = new AirportInfoETA
            {
                Airline_Id = "SW",
                Flight_No = "SW3398",
                Estimated_Arrival = "12:33"
            };

            MessageQueue messageQueue = null;
            if (!MessageQueue.Exists(@".\Private$\L09FlightETAQueue"))
            {
                // Opret Queue hvis den ikke eksisterer i forvejen
                MessageQueue.Create(@".\Private$\L09FlightETAQueue");
            }
            messageQueue = new MessageQueue(@".\Private$\L09FlightETAQueue");
            messageQueue.Label = "FlightInfoETAQueue";

            // sender SAS flight info
            string SASjsonString = JsonSerializer.Serialize(SAS_AirportInfoETA);
            string AirlineCompany = "SAS_ETA";
            messageQueue.Send(SASjsonString, AirlineCompany);

            List<String> strings = new List<String>();
            strings.Add(messageQueue.Receive().Body.ToString());
            
            // sender KLM flight info
            string KLMjsonString = JsonSerializer.Serialize(KLM_AirportInfoETA);
            AirlineCompany = "KLM_ETA";
            messageQueue.Send(KLMjsonString, AirlineCompany);
            strings.Add(messageQueue.Receive().Body.ToString());

            // sender SW flight info
            string SWjsonString = JsonSerializer.Serialize(SW_AirportInfoETA);
            AirlineCompany = "SW_ETA";
            messageQueue.Send(SWjsonString, AirlineCompany);
            strings.Add(messageQueue.Receive().Body.ToString());

            Console.WriteLine(concatStrings(strings));

            Console.ReadLine();
        }

        public static string concatStrings(List<string> strings) 
        { 
            StringBuilder sb = new StringBuilder();
            foreach (string s in strings) 
            { 
                sb.Append(s);
            }
            return sb.ToString();
        }

        // TODO: re-write method as formatting method and use before adding the string to the List!!!
        public static string[] extractInfo(string message)
        {
            string txt = Regex.Replace(message, "\"", " ");
            txt = Regex.Replace(txt, "[^\\w\\. _]", "");
            return txt.Split(' ');
        }
    }
}
