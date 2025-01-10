using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Messaging;

namespace EksamensprojektOpg3_2
{
    internal class Program
    {
        // Delopgave 3.2
        // Der skal kunne tjekkes, om den, der ønsker at tilmelde sig en aktivitet, er medlem.

        // queues
        private static MessageQueue tilmeldingssystemQueue;
        private static MessageQueue medlemsregisterQueue;

        static void Main(string[] args)
        {
            // opretter queues
            CreateQueues();
            String request = @".\Private$\L21MedlemsregisterQueue";
            String reply = @".\Private$\L21TilmeldingssystemQueue";

            // opretter objekter der repræsenterer vores 2 systemer
            Requestor req1 = new Requestor(request, reply, "22334");    // request for medlem
            Requestor req2 = new Requestor(request, reply, "98765");    // request for ikke-medlem
            Requestor req3 = new Requestor(request, reply, "22334");    // request for medlem
            Replier replier = new Replier(request);

            // sender nogle requests og får nogle replies tilbage (med simulering af lidt ventetid mellem beskederne :) )
            req1.Send();
            Thread.Sleep(5000);

            req2.Send();
            Thread.Sleep(5000);

            req3.Send();

            while (true) { }
        }

        private static void CreateQueues()
        {
            if (!MessageQueue.Exists(@".\Private$\L21TilmeldingssystemQueue"))
            {
                MessageQueue.Create(@".\Private$\L21TilmeldingssystemQueue");
            }
            tilmeldingssystemQueue = new MessageQueue(@".\Private$\L21TilmeldingssystemQueue");
            tilmeldingssystemQueue.Label = "Tilmeldingssystem Queue";

            if (!MessageQueue.Exists(@".\Private$\L21MedlemsregisterQueue"))
            {
                MessageQueue.Create(@".\Private$\L21MedlemsregisterQueue");
            }
            medlemsregisterQueue = new MessageQueue(@".\Private$\L21MedlemsregisterQueue");
            medlemsregisterQueue.Label = "Medlemsregister Queue";
        }
    }
}
