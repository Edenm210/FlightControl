using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public class Location
    {
        public Location(double longi, double lati)
        {
            Longitude = longi;
            Latitude = lati;
        }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }
}
