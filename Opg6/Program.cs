using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace L02Opg6
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // create or connect to queues
            MessageQueue airportQueue = CreateQueue(@".\Private$\AirportGateQueue");
            MessageQueue sasQueue = CreateQueue(@".\Private$\SASGateQueue");
            MessageQueue klmQueue = CreateQueue(@".\Private$\KLMGateQueue");
            MessageQueue swaQueue = CreateQueue(@".\Private$\SWAGateQueue");

            // create a GateInfoRouter to manage the routing between the queues
            GateInfoRouter router = new GateInfoRouter(airportQueue, sasQueue, klmQueue, swaQueue);

            // send test messages to the AirportGateQueue
            SendGateInfo(airportQueue, "SK123", "Copenhagen", "2024-08-26T15:30:00Z", "2024-08-26T15:00:00Z", "B12");
            SendGateInfo(airportQueue, "KL456", "Amsterdam", "2024-08-26T17:45:00Z", "2024-08-26T17:15:00Z", "C34");
            SendGateInfo(airportQueue, "SW789", "New York", "2024-08-26T19:00:00Z", "2024-08-26T18:30:00Z", "D56");

            Console.WriteLine("Gate info messages sent to AirportGateQueue");
            Console.ReadLine();
        }

        static MessageQueue CreateQueue(string path)
        {
            if (!MessageQueue.Exists(path))
            {
                return MessageQueue.Create(path);
            }
            else
            {
                return new MessageQueue(path);
            }
        }

        static void SendGateInfo(MessageQueue queue, string flightNo, string destination, string departureTime, string boardingTime, string gateNo)
        {
            var gateInfo = new GateInfo
            {
                FlightNo = flightNo,
                Destination = destination,
                DepartureTime = departureTime,
                BoardingTime = boardingTime,
                GateNo = gateNo
            };

            var message = new System.Messaging.Message(gateInfo)
            {
                Formatter = new XmlMessageFormatter(new Type[] { typeof(GateInfo) })
            };

            queue.Send(message);
        }
    }
}
