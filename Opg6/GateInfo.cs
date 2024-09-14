using System;

namespace L02Opg6
{
    [Serializable]
    public class GateInfo
    {
        public string FlightNo { get; set; }
        public string Destination { get; set; }
        public string DepartureTime { get; set; }
        public string BoardingTime { get; set; }
        public string GateNo { get; set; }
    }
}
