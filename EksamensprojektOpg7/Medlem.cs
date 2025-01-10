using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EksamensprojektOpg7
{
    public class Medlem
    {
        private CultureInfo danishCulture = new CultureInfo("da-DK");
        private DateTime? datoBrev;
        public ushort MedlemsNummer { get; set; }
        public String Fornavn { get; set; }
        public String Efternavn { get; set; }
        public String Adresselinie1 { get; set; }
        public String Adresselinie2 { get; set; }
        public String PostNr { get; set; }
        public String ByNavn { get; set; }
        public String MailAdresse { get; set; }
        public bool ErPrivat { get; set; } = true;
        public bool ErAktiv { get; set; } = true;
        public bool HarBetaltKontingent { get; set; } = true;
        public bool ErTilmeldtNyhedsbrev { get; set; } = false;

        // DatoBrev property
        public string DatoBrev
        {
            get
            {
                // format the date when retrieved or return null if no date is set
                return datoBrev.HasValue ? datoBrev.Value.ToString("dd. MMMM yyyy", danishCulture) : null;
            }
            set
            {
                // try to parse the string into a DateTime in the format dd.måned.åååå. f.eks. 23. oktober 2024
                if (DateTime.TryParseExact(value, "dd. MMMM yyyy", danishCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    datoBrev = parsedDate;
                }
                else
                {
                    throw new FormatException($"DatoBrev must be in the format 'dd. MMMM yyyy'. Provided value: '{value}'");
                }
            }
        }
    }
}
