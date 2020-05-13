using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace FlightControlWeb.Models
{
    public class InitialFlightLocation
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        [JsonPropertyName("date_time")]
        public DateTime Date_Time { get; set; }
    }
}
