using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FlightControlWeb.Models
{
    public class FlightPlan
    {
        [Key]
        [JsonIgnore]
        public string FlightId { get; set; }
        [JsonPropertyName("passengers")]
        public int Passengers { get; set; }
        [JsonPropertyName("company_name")]
        public string CompanyName { get; set; }
        [JsonPropertyName("initial_location")]
        public InitialFlightLocation InitialLocation { get; set; }
        [JsonPropertyName("segments")]
        [NotMapped]
        public List<FlightSegment> Segments { get; set; }
    }
}
