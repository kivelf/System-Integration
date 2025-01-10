using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EksamensprojektOpg7
{
    internal class Program
    {
        // Opgave 7
        // Implementerer oversættelsen (translation) mellem systemerne Medlem og Nyhedsbrev.

        // queues and objects representing our systems
        private static MessageQueue outQueue;   // beskeder fra Medlemsregisteret bliver først sendt her
        private static MessageQueue translatedMessageQueue; // translated messages som går til Nyhedsbrevsystemet
        private static Medlemsregister medlemsregister;
        private static Nyhedsbrev nyhedsbrev;
        private static Translator translator;
        private static List<Medlem> medlemmer = new List<Medlem>();

        static void Main(string[] args)
        {
            // opretter queues
            CreateQueues();

            // opretter objekter, som repræsenterer vores systemer
            medlemsregister = new Medlemsregister(outQueue);
            nyhedsbrev = new Nyhedsbrev(translatedMessageQueue);
            translator = new Translator(outQueue, translatedMessageQueue);

            // opretter nogle medlemmer (obs, at der bliver oprettet 5 medlemmer, men kun 3 er tilmeldt nyhedsbrevet!)
            CreateMembers();
            foreach (var member in medlemmer)
            {
                medlemsregister.AddMedlem(member);
            }

            medlemsregister.PrintMedlemmer();
            Console.WriteLine("Retrieving newsletter subscriber list, please wait...");
            Thread.Sleep(3000);
            nyhedsbrev.PrintNewsletterSubscribers();

            while (true) { }
        }

        private static void CreateQueues()
        {
            if (!MessageQueue.Exists(@".\Private$\L21OutQueue"))
            {
                MessageQueue.Create(@".\Private$\L21OutQueue");
            }
            outQueue = new MessageQueue(@".\Private$\L21OutQueue");
            outQueue.Label = "Out Queue";

            if (!MessageQueue.Exists(@".\Private$\L21TranslatedMessageQueue"))
            {
                MessageQueue.Create(@".\Private$\L21TranslatedMessageQueue");
            }
            translatedMessageQueue = new MessageQueue(@".\Private$\L21TranslatedMessageQueue");
            translatedMessageQueue.Label = "Medlemsregister Queue";
        }

        private static void CreateMembers()
        {
            Medlem medlem1 = new Medlem
            {
                MedlemsNummer = 12345,
                Fornavn = "John",
                Efternavn = "Doe",
                Adresselinie1 = "Nygade 123",
                Adresselinie2 = null,
                PostNr = "2200",
                ByNavn = "København N",
                MailAdresse = "john@doe.dk",
                ErTilmeldtNyhedsbrev = true,
                DatoBrev = "23. oktober 2024"
            };

            Medlem medlem2 = new Medlem
            {
                MedlemsNummer = 22334,
                Fornavn = "Bob",
                Efternavn = "Bobsson",
                Adresselinie1 = "Gamlegade 321",
                Adresselinie2 = "st.th.",
                PostNr = "5000",
                ByNavn = "Odense C",
                MailAdresse = "bob@thebob.dk",
                ErTilmeldtNyhedsbrev = true,
                DatoBrev = "29. november 2024"
            };

            Medlem medlem3 = new Medlem
            {
                MedlemsNummer = 33445,
                Fornavn = "Bobsine",
                Efternavn = "Bobsson",
                Adresselinie1 = "Gamlegade 321",
                Adresselinie2 = "st.th.",
                PostNr = "5000",
                ByNavn = "Odense C",
                MailAdresse = "bobsine@thebob.dk"
            };

            Medlem medlem4 = new Medlem
            {
                MedlemsNummer = 54321,
                Fornavn = "Jim",
                Efternavn = "Doe",
                Adresselinie1 = "Langevej 456",
                PostNr = "8000",
                ByNavn = "Aarhus C",
                MailAdresse = "jim@doe.dk",
                ErTilmeldtNyhedsbrev = true,
                DatoBrev = "18. august 2023"
            };

            Medlem medlem5 = new Medlem
            {
                MedlemsNummer = 56789,
                Fornavn = "Jimbob",
                Efternavn = "Bobsson",
                Adresselinie1 = "Kortevej 3",
                Adresselinie2 = "st.tv.",
                PostNr = "8200",
                ByNavn = "Aarhus N",
                MailAdresse = "jimbob@bobjim.dk"
            };

            medlemmer.Add(medlem1);
            medlemmer.Add(medlem2);
            medlemmer.Add(medlem3);
            medlemmer.Add(medlem4);
            medlemmer.Add(medlem5);
        }
    }
}
