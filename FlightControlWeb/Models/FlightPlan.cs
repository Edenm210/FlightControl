using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightControlWeb.Models
{
    public class FlightPlan
    {
        //[JsonIgnore]
        //public string Flight_Id { get; set; }
        [JsonPropertyName("passengers")]
        public int Passengers { get; set; }
        [JsonPropertyName("company_name")]
        public string Company_Name { get; set; }
        [JsonPropertyName("initial_location")]
        public InitialFlightLocation Initial_Location { get; set; }
        [JsonPropertyName("segments")]
        public List<FlightSegment> Segments { get; set; }
    }
}
