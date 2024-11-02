using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace L16Opg3
{
    public class Passenger
    {
        public String Name { get; set; }
        public String TicketNo { get; set; }
        public String PassportNo { get; set; }
        public String FlightNo { get; set; }
    }

    internal class Program
    {
        private static MessageQueue inQueue;
        static void Main(string[] args)
        {
            var passenger1 = new Passenger
            {
                Name = "Anders And",
                TicketNo = "CA937200305251",
                PassportNo = "200305252",
                FlightNo = "SAS1497"
            };

            var passenger2 = new Passenger
            {
                Name = "Bob Bobsson",
                TicketNo = "CA937200305248",
                PassportNo = "200305263",
                FlightNo = "SW497"
            };

            var passenger3 = new Passenger
            {
                Name = "Bobsine Bobsson",
                TicketNo = "CA937200305247",
                PassportNo = "200301234",
                FlightNo = "SW497"
            };
            List<Passenger> passengers = new List<Passenger>();
            passengers.Add(passenger1);
            passengers.Add(passenger2);
            passengers.Add (passenger3);

            // creating the queue
            if (!MessageQueue.Exists(@".\Private$\L16InQueue"))
            {
                MessageQueue.Create(@".\Private$\L16InQueue");
            }
            inQueue = new MessageQueue(@".\Private$\L16InQueue");
            inQueue.Label = "In Queue";
            inQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnMessageReceived);
            inQueue.BeginReceive();
            Console.WriteLine("Listening for messages...");


            foreach (Passenger passenger in passengers) 
            {
                CheckingPassenger(passenger);
            }

            while (true) { }
        }

        private static void OnMessageReceived(object sender, ReceiveCompletedEventArgs e)
        {
            Message m = inQueue.Peek();
            if (m.Label.Contains("SW"))
            {
                // process message
                string id = m.Id;
                inQueue.ReceiveById(id);
                Console.WriteLine($"Message with id {id} read.");
            }
        }

        public static void CheckingPassenger(Passenger p)
        {
            // generate a message with the passenger info
            string passengerInfo = "Name: " + p.Name + "\nTicket Number: " + p.TicketNo
                + "\nPassport number: " + p.PassportNo + "\nFlight number: " + p.FlightNo;

            // string for the label
            string passengerInfoShort = p.FlightNo;

            Message msg = new Message
            {
                Body = passengerInfo,
                Label = "Passenger checked in: " + passengerInfoShort,
                Formatter = new XmlMessageFormatter(new Type[] { typeof(XmlDocument) })
            };

            inQueue.Send(msg);
        }
    }
}
