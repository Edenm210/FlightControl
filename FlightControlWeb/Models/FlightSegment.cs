using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace FlightControlWeb.Models
{
    public class FlightSegment
    {
        public FlightSegment()
        {
            // Set default as invalid values to the fields of numbers for validation checking.
            Latitude = 100;
            Longitude = 200;
            TimespanSeconds = -1;
        }

        [Key]
        [JsonIgnore]
        public int Id { get; set; }
        [JsonIgnore]
        public string FlightId { get; set; }
        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }
        [JsonPropertyName("timespan_seconds")]
        public int TimespanSeconds { get; set; }
    }
}
