using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public class Flight
    {
        public Flight(string flightId, double longi, double lati, int numPassingers, string companyName, DateTime time)
        {
            Flight_id = flightId;
            Longitude = longi;
            Latitude = lati;
            Passengers = numPassingers;
            Company_name = companyName;
            Date_time = time;
            Is_external = false;
        }
        public string Flight_id { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public int Passengers { get; set; }
        public string Company_name { get; set; }
        public DateTime Date_time { get; set; }
        public bool Is_external { get; set; }

        public void UpdateExternal(bool isExternal)
        {
            Is_external = isExternal;
        }
    }
}
