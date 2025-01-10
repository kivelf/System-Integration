using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EksamensprojektOpg7
{
    public class Nyhedsbrev
    {
        protected MessageQueue InQueue;
        List<NewsletterSubscriber> Subscribers = new List<NewsletterSubscriber>();

        public Nyhedsbrev(MessageQueue inQueue)
        {
            InQueue = inQueue;

            InQueue.Formatter = new XmlMessageFormatter(new string[] { "System.String,mscorlib" });
            InQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnMessage);
            InQueue.BeginReceive();
        }

        public void OnMessage(Object source, ReceiveCompletedEventArgs asyncResult)
        {
            // modtager beskeden
            MessageQueue mq = (MessageQueue)source;
            Message message = mq.EndReceive(asyncResult.AsyncResult);

            // read the content from the original message
            string XMLDocument;
            using (Stream body = message.BodyStream)
            using (StreamReader reader = new StreamReader(body))
            {
                XMLDocument = reader.ReadToEnd();
            }

            // extracts the needed data
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(XMLDocument);
            string membershipNumber = xml.SelectSingleNode("Member/MembershipNumber").InnerText;
            string name = xml.SelectSingleNode("Member/Name").InnerText;
            string address = xml.SelectSingleNode("Member/Address").InnerText;
            string zipCode = xml.SelectSingleNode("Member/ZipCode").InnerText;
            string mail = xml.SelectSingleNode("Member/Mail").InnerText;
            string dato = xml.SelectSingleNode("Member/DateRegistered").InnerText;

            // create new Newsletter Subscriber
            NewsletterSubscriber subscriber = new NewsletterSubscriber();
            subscriber.MembershipNumber = membershipNumber;
            subscriber.Name = name;
            subscriber.Address = address;
            subscriber.ZipCode = Int32.Parse(zipCode);
            subscriber.Mail = mail;
            subscriber.SetAmericanDateFromString(dato);

            Subscribers.Add(subscriber);

            // continue listening for messages
            InQueue.BeginReceive();
        }

        public void PrintNewsletterSubscribers()
        {
            Console.WriteLine("Newsletter subscribers:");
            foreach (NewsletterSubscriber s in Subscribers)
            {
                Console.WriteLine(s.MembershipNumber + " " + s.Name + ", subscribed since: " + s.AmericanDate.ToString("MM/dd/yyyy"));
            }
            Console.WriteLine("------------------------------");
        }
    }
}
