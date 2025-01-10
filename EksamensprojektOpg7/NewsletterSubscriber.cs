using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EksamensprojektOpg7
{
    public class NewsletterSubscriber
    {
        public string MembershipNumber { get; set; }
        public String Name { get; set; }
        public String Address { get; set; }
        public int ZipCode { get; set; }
        public String Mail { get; set; }

        // internal storage for the American date
        private DateTime americanDate;

        // property to get or set the American date as a DateTime
        public DateTime AmericanDate
        {
            get => americanDate;
        }

        // method to set the American date from a string
        public void SetAmericanDateFromString(string americanDateString)
        {
            if (string.IsNullOrWhiteSpace(americanDateString))
            {
                throw new ArgumentException("The provided American date string is null or empty.");
            }

            try
            {
                // parse the American date string into a DateTime object and store it
                americanDate = DateTime.ParseExact(americanDateString, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                throw new FormatException($"The date '{americanDateString}' is not in the expected format 'MM/dd/yyyy'.");
            }
        }
    }
}
