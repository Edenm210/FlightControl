using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace FlightControlWeb.Models
{
    public class InitialFlightLocation
    {
        public InitialFlightLocation()
        {
            // Set default as invalid values to the fields of numbers for validation checking.
            Latitude = 100;
            Longitude = 200;
        }
        [Key]
        [JsonIgnore]
        public string Id { get; set; }
        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }
        [JsonPropertyName("date_time")]
        public DateTime DateTime { get; set; }
    }
}
