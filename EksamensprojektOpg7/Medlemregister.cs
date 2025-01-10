using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EksamensprojektOpg7
{
    public class Medlemsregister
    {
        List<Medlem> Medlemmer = new List<Medlem>();
        private MessageQueue OutQueue;

        public Medlemsregister(MessageQueue outQueue)
        {
            OutQueue = outQueue;
        }

        public void AddMedlem(Medlem medlem)
        {
            Medlemmer.Add(medlem);

            if (medlem.ErTilmeldtNyhedsbrev)
            {
                NotifyNyhedsbrev(medlem);
            }
        }

        private void NotifyNyhedsbrev(Medlem medlem)
        {
            // construct message with the relevant data
            // create the XmlDocument object
            XmlDocument xmlDocument = new XmlDocument();
            // root element
            XmlElement medlemElement = xmlDocument.CreateElement("Medlem");
            // children elements
            XmlElement medlemsnummerElement = xmlDocument.CreateElement("Medlemsnummer");
            medlemsnummerElement.InnerText = medlem.MedlemsNummer.ToString();
            XmlElement fornavnElement = xmlDocument.CreateElement("Fornavn");
            fornavnElement.InnerText = medlem.Fornavn;
            XmlElement efternavnElement = xmlDocument.CreateElement("Efternavn");
            efternavnElement.InnerText = medlem.Efternavn;
            XmlElement adresseElement = xmlDocument.CreateElement("Adresse");
            adresseElement.InnerText = medlem.Adresselinie1;
            if (medlem.Adresselinie2 != null)
            {
                adresseElement.InnerText += "\n" + medlem.Adresselinie2;
            }
            XmlElement byNavnElement = xmlDocument.CreateElement("ByNavn");
            byNavnElement.InnerText = medlem.ByNavn;
            XmlElement postNrElement = xmlDocument.CreateElement("PostNr");
            postNrElement.InnerText = medlem.PostNr;
            XmlElement mailAdresseElement = xmlDocument.CreateElement("MailAdresse");
            mailAdresseElement.InnerText = medlem.MailAdresse;
            XmlElement datoBrevElement = xmlDocument.CreateElement("DatoBrev");
            datoBrevElement.InnerText = medlem.DatoBrev.ToString();

            // append children elements to the root element
            medlemElement.AppendChild(medlemsnummerElement);
            medlemElement.AppendChild(fornavnElement);
            medlemElement.AppendChild(efternavnElement);
            medlemElement.AppendChild(adresseElement);
            medlemElement.AppendChild(byNavnElement);
            medlemElement.AppendChild(postNrElement);
            medlemElement.AppendChild(mailAdresseElement);
            medlemElement.AppendChild(datoBrevElement);
            // append the root element to the XmlDocument
            xmlDocument.AppendChild(medlemElement);

            Message subscribeMessage = new Message
            {
                Body = xmlDocument,
                Label = "Nyt medlem til Nyhedsbrevet",
                Formatter = new XmlMessageFormatter(new string[] { "System.String,mscorlib" })
            };

            // send to the out queue
            try
            {
                OutQueue.Send(subscribeMessage);
            }
            catch (MessageQueueException e)
            {
                Console.WriteLine("Error sending message: " + e.Message);
            }
        }

        public void PrintMedlemmer()
        {
            Console.WriteLine("Medlemmer i Medlemsregisteret:");
            foreach (Medlem m in Medlemmer)
            {
                Console.WriteLine(m.MedlemsNummer + " " + m.Fornavn + " " + m.Efternavn + ", nyhedsbrev: " + m.ErTilmeldtNyhedsbrev);
            }
            Console.WriteLine("------------------------------");
        }
    }
}
