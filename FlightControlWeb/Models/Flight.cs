using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public class Flight
    {
        public Flight() 
        {
            // Set default as invalid values to the fields of numbers for validation checking.
            Passengers = -1;
            Latitude = 100;
            Longitude = 200;
        }

        public Flight(string flightId, double longi, double lati, int numPassingers,
            string companyName, DateTime time)
        {
            FlightId = flightId;
            Longitude = longi;
            Latitude = lati;
            Passengers = numPassingers;
            CompanyName = companyName;
            DateTime = time;
            IsExternal = false;
        }

        [JsonPropertyName("flight_id")]
        public string FlightId { get; set; }
        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }
        [JsonPropertyName("passengers")]
        public int Passengers { get; set; }
        [JsonPropertyName("company_name")]
        public string CompanyName { get; set; }
        [JsonPropertyName("date_time")]
        public DateTime DateTime { get; set; }
        [JsonPropertyName("is_external")]
        public bool IsExternal { get; set; }
    }
}
