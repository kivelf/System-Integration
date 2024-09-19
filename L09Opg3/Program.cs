using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace L09Opg3
{
    public class AirportInfoETA
    {
        public String Flight_No { get; set; }
        public String Estimated_Arrival { get; set; }
    }
    public class Program
    {
        public static void Main(string[] args)
        {
            Dictionary<string, string> flightETAPairs = new Dictionary<string, string>();
            List<String> strings = new List<String>();

            var SAS_AirportInfoETA1 = new AirportInfoETA
            {
                Flight_No = "SK952",
                Estimated_Arrival = "11:46"
            };

            var SAS_AirportInfoETA2 = new AirportInfoETA
            {
                Flight_No = "SK264",
                Estimated_Arrival = "15:18"
            };

            var SAS_AirportInfoETA3 = new AirportInfoETA
            {
                Flight_No = "SK398",
                Estimated_Arrival = "22:32"
            };

            MessageQueue messageQueue = null;
            if (!MessageQueue.Exists(@".\Private$\L09FlightETAQueue"))
            {
                // Opret Queue hvis den ikke eksisterer i forvejen
                MessageQueue.Create(@".\Private$\L09FlightETAQueue");
            }
            messageQueue = new MessageQueue(@".\Private$\L09FlightETAQueue");
            messageQueue.Label = "FlightInfoETAQueue";

            // sender SAS flight info 1
            string SASjsonString1 = JsonSerializer.Serialize(SAS_AirportInfoETA1);
            string AirlineCompany = "SAS_ETA1";
            messageQueue.Send(SASjsonString1, AirlineCompany);
            strings.Add(messageQueue.Receive().Body.ToString());

            // sender SAS flight info 2
            string SASjsonString2 = JsonSerializer.Serialize(SAS_AirportInfoETA2);
            AirlineCompany = "SAS_ETA2";
            messageQueue.Send(SASjsonString2, AirlineCompany);
            strings.Add(messageQueue.Receive().Body.ToString());

            // sender SAS flight info 3
            string SASjsonString3 = JsonSerializer.Serialize(SAS_AirportInfoETA3);
            AirlineCompany = "SAS_ETA3";
            messageQueue.Send(SASjsonString3, AirlineCompany);
            strings.Add(messageQueue.Receive().Body.ToString());

            // adding the message data to our dictionary
            foreach (string str in strings) 
            {
                processETAData(str, flightETAPairs);
            }

            foreach (KeyValuePair<string, string> kvp in flightETAPairs) 
            {
                Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
            }

            // writing the data from the dictionary to an Excel sheet
            sendDataToExcel(flightETAPairs);

            Console.ReadLine();
        }

        public static void processETAData(string message, Dictionary<string, string> flightsETA)
        {
            string[] data = message.Split('"');
            string flight = "";
            string ETA = "";
            foreach (string str in data) 
            {
                if (str.Contains("SK"))
                {
                    flight = str;
                }
                else if (Regex.Replace(str, "[^\\w\\. _]", "").All(Char.IsDigit) && str.Length > 1)
                { 
                    ETA = str;
                    Console.WriteLine(ETA);
                }
            }

            flightsETA.Add(flight, ETA);
        }

        public static void sendDataToExcel(Dictionary<string, string> flightsETA)
        {
            Excel.Application oXL;
            Excel._Workbook oWB;
            Excel._Worksheet oSheet;
            Excel.Range oRng;

            try
            {
                //Start Excel and get Application object.
                oXL = new Excel.Application();
                oXL.Visible = true;

                //Get a new workbook.
                oWB = (Excel._Workbook)(oXL.Workbooks.Add(Missing.Value));
                oSheet = (Excel._Worksheet)oWB.ActiveSheet;

                //Add table headers going cell by cell.
                oSheet.Cells[1, 1] = "Flight";
                oSheet.Cells[1, 2] = "ETA";

                //Format A1:B1 as bold, vertical alignment = center.
                oSheet.get_Range("A1", "B1").Font.Bold = true;
                oSheet.get_Range("A1", "B1").VerticalAlignment =
                    Excel.XlVAlign.xlVAlignCenter;

                // Create an array to multiple values at once.
                string[,] ETA = new string[flightsETA.Count, 2];
                
                // fill the array with the flights ETA data
                int i = 0;
                foreach (KeyValuePair<string, string> kvp in flightsETA)
                {
                    ETA[i, 0] = kvp.Key;
                    ETA[i, 1] = kvp.Value;
                    i++;
                }

                //Fill A2:B4 with an array of values (Flights).
                oSheet.get_Range("A2", "B4").Value2 = ETA;

                //AutoFit columns A:B.
                oRng = oSheet.get_Range("A1", "B1");
                oRng.EntireColumn.AutoFit();

                //Make sure Excel is visible and give the user control
                //of Microsoft Excel's lifetime.
                oXL.Visible = true;
                oXL.UserControl = true;
            }
            catch (Exception theException)
            {
                Console.WriteLine("Error! " + theException);
            }
        }

    }
}
