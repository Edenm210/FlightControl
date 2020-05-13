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
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        [JsonPropertyName("timespan_seconds")]
        public int Timespan_Seconds { get; set; }
    }
}
