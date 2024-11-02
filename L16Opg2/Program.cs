using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        private static CheckInEmployee checkInEmployee;
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

            // create a checkInEmployee instance and check in the 3 passengers
            checkInEmployee = new CheckInEmployee(checkInQueue);
            Thread.Sleep(new Random().Next(5000, 10000));
            checkInEmployee.CheckingPassenger(passenger1);
            Thread.Sleep(new Random().Next(5000, 10000));
            checkInEmployee.CheckingPassenger(passenger2);
            Thread.Sleep(new Random().Next(5000, 10000));
            checkInEmployee.CheckingPassenger(passenger3);

            Console.WriteLine("Listening for messages...");
            while (true) { }
        }

        private static void OnMessageReceived(object sender, ReceiveCompletedEventArgs e)
        {
            MessageQueue mq = (MessageQueue)sender;
            Message receivedMsg = mq.EndReceive(e.AsyncResult);

            // use the existing splitter instance to process the received message
            splitter.OnMessage(receivedMsg);

            // start listening for the next message
            mq.BeginReceive();
        }
    }
}
