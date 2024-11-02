using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Xml;

namespace L16Opg2
{
    internal class CheckInEmployee
    {
        protected MessageQueue checkInQueue;

        public CheckInEmployee(MessageQueue checkInQueue)
        {
            this.checkInQueue = checkInQueue;
        }

        public void CheckingPassenger(Passenger p) 
        {
            if (VerifyingTicket()) 
            {
                // generate a message with the passenger info
                string passengerInfo = "Name: " + p.Name + "\nTicket Number: " + p.TicketNo 
                    + "\nPassport number: " + p.PassportNo + "\nFlight number: " + p.FlightNo;
                
                Message msg = new Message
                {
                    Body = passengerInfo,
                    Label = "From Bluff City International Airport Customs & Border Protection",
                    Formatter = new XmlMessageFormatter(new Type[] { typeof(XmlDocument) })
                };

                checkInQueue.Send(msg);
            }
        }

        public bool VerifyingTicket() 
        {
            // generic implementation
            return true;
        }
    }
}
