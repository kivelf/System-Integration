using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace L16Opg2
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
        private static MessageQueue checkInQueue;
        static void Main(string[] args)
        {
            var passenger1 = new Passenger
            {
                Name = "Anders And",
                TicketNo = "CA937200305251",
                PassportNo = "200305252",
                FlightNo = "SW497"
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

            // creating the queue
            if (!MessageQueue.Exists(@".\Private$\L16AirportCheckInQueue"))
            {
                MessageQueue.Create(@".\Private$\L16AirportCheckInQueue");
            }
            checkInQueue = new MessageQueue(@".\Private$\L16AirportCheckInQueue");
            checkInQueue.Label = "CheckIn Queue";
            // listening for incoming messages from the CheckInOutput queue
            checkInQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnMessageReceived);
            checkInQueue.BeginReceive();
            Console.WriteLine("Listening for messages...");

            // check in the 3 passengers
            Thread.Sleep(new Random().Next(5000, 10000));
            CheckingPassenger(passenger1);
            Thread.Sleep(new Random().Next(5000, 10000));
            CheckingPassenger(passenger2);
            Thread.Sleep(new Random().Next(5000, 10000));
            CheckingPassenger(passenger3);

            Console.ReadLine();
        }

        private static void OnMessageReceived(object sender, ReceiveCompletedEventArgs e)
        {
            MessageQueue mq = (MessageQueue)sender;
            Message receivedMsg = mq.EndReceive(e.AsyncResult);

            // process the message
            Console.WriteLine(receivedMsg.Label);

            checkInQueue.BeginReceive();
        }

        public static void CheckingPassenger(Passenger p)
        {
            // generate a message with the passenger info
            string passengerInfo = "Name: " + p.Name + "\nTicket Number: " + p.TicketNo
                + "\nPassport number: " + p.PassportNo + "\nFlight number: " + p.FlightNo;

            // string for the label
            string passengerInfoShort = "Name: " + p.Name + "\nTicket Number: " + p.TicketNo;

            Message msg = new Message
            {
                Body = passengerInfo,
                Label = "Passenger checked in: " + passengerInfoShort,
                Formatter = new XmlMessageFormatter(new Type[] { typeof(XmlDocument) })
            };

            checkInQueue.Send(msg);
        }
    }
}
