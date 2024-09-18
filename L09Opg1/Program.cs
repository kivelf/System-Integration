using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace L09Opg1
{
    public class AirportInfoETA
    {
        public String Flight_No { get; set; }
        public String Estimated_Arrival { get; set; }
    }
    public class Program
    {
        public static void Main(string[] args)
        {
            var flightInfoETA = new AirportInfoETA
            {
                Flight_No = "SK952",
                Estimated_Arrival = "10:49"
            };

            MessageQueue messageQueue = null;
            if (!MessageQueue.Exists(@".\Private$\L09TTLqueue"))
            {
                // Opret Queue hvis den ikke eksisterer i forvejen
                MessageQueue.Create(@".\Private$\L09TTLqueue");
            }
            messageQueue = new MessageQueue(@".\Private$\L09TTLqueue");
            messageQueue.Label = "TTLqueue";

            // sender besked med TTL
            string jsonString = JsonSerializer.Serialize(flightInfoETA);
            string AirlineCompany = "TTL_Test";
            Message msg = new Message();
            msg.Body = jsonString;
            msg.Label = AirlineCompany;
            msg.TimeToBeReceived = TimeSpan.FromSeconds(60);
            messageQueue.Send(msg);
            Console.WriteLine("Message sent to TTL Queue. TTL: 60 sec");

            Console.ReadLine();
        }
    }
}
