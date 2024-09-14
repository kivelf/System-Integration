using SysIntegration1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Opg5
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // creating or connecting to queues
            MessageQueue airportQueue = CreateQueue(@".\Private$\AirportQueue");
            MessageQueue sasQueue = CreateQueue(@".\Private$\SASQueue");
            MessageQueue swaQueue = CreateQueue(@".\Private$\SWAQueue");

            // creating a SimpleRouter to manage the routing between the queues
            SimpleRouter router = new SimpleRouter(airportQueue, sasQueue, swaQueue);

            // sending a message to the AirportQueue
            SendFlightInfo(airportQueue, "Scandinavian Airline Service", "SK123", "2024-08-26T15:30:00Z", "Copenhagen", true);
            SendFlightInfo(airportQueue, "South West Airlines", "SW456", "2024-08-26T17:45:00Z", "New York", false);

            Console.WriteLine("Messages sent to AirportQueue");
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

        static void SendFlightInfo(MessageQueue queue, string airline, string flightNo, string scheduledTime, string destination, bool checkIn)
        {
            var flightInfo = new FlightInfo
            {
                Airline = airline,
                FlightNumber = flightNo,
                ScheduledTime = scheduledTime,
                Destination = destination,
                CheckIn = checkIn
            };

            var message = new System.Messaging.Message(flightInfo)
            {
                Formatter = new XmlMessageFormatter(new Type[] { typeof(FlightInfo) })
            };

            queue.Send(message);
        }
    }
}
